using Orleans.Runtime;

namespace ContosoLoans.LoanReception
{
    public class LoanProcessOrchestratorGrain : Grain, ILoanProcessOrchestratorGrain
    {
        private readonly ILogger<LoanProcessOrchestratorGrain> _logger;
        private readonly IPersistentState<List<LoanApplication>> _state;

        public LoanProcessOrchestratorGrain(ILogger<LoanProcessOrchestratorGrain> logger,
            [PersistentState("LoanApplicationsInProgress")]
            IPersistentState<List<LoanApplication>> state)
        {
            _logger = logger;
            _state = state;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await _state.ReadStateAsync();
            RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await _state.WriteStateAsync();
        }

        private async Task OnTimer(object state)
        {
            (await GetLoansInProgress()).ToList().ForEach(async x =>
            {
                _logger.LogInformation($"Checking status of loan application {x.ApplicationId}.");

                var loanAppGrain = GrainFactory.GetGrain<ILoanApplicationGrain>(x.ApplicationId);
                await loanAppGrain.CheckCredit();
            });
        }

        public async Task<List<LoanApplication>> GetLoansInProgress()
        {
            return _state.State;
        }

        public async Task OnLoanApplicationProcessed(LoanApplication app)
        {
            _state.State.RemoveAll(x => x.ApplicationId == app.ApplicationId);

            _logger.LogInformation($"Loan application {app.ApplicationId} processed. Result: {app.IsApproved}");
        }

        public async Task StartEvaluation(LoanApplication app)
        {
            _state.State.Add(app);
            await _state.WriteStateAsync();

            var loanAppGrain = GrainFactory.GetGrain<ILoanApplicationGrain>(app.ApplicationId);
            await loanAppGrain.Set(app);
        }
    }
}
