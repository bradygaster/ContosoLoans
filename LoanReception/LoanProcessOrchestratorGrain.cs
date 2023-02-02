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

        public async Task<List<LoanApplication>> GetLoansInProgress()
        {
            await _state.ReadStateAsync();
            return _state.State;
        }

        public async Task OnLoanApplicationProcessed(LoanApplication app)
        {
            _state.State.RemoveAll(x => x.ApplicationId == app.ApplicationId);
            await _state.WriteStateAsync();
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
