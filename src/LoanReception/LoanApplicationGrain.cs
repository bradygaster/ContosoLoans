using Orleans.Runtime;

namespace ContosoLoans.LoanReception {
    [CollectionAgeLimit(Minutes = 2)]
    public class LoanApplicationGrain : Grain, ILoanApplicationGrain {
        private readonly ILogger<LoanApplicationGrain> _logger;
        private readonly IPersistentState<LoanApplication> _state;
        private IDisposable _timerHandle;

        public LoanApplicationGrain(ILogger<LoanApplicationGrain> logger,
            [PersistentState("LoanApplicationsInProgress")]
            IPersistentState<LoanApplication> state) {
            _logger = logger;
            _state = state;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken) {
            await _state.ReadStateAsync();
            _timerHandle = base.RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken) {
            await _state.WriteStateAsync();
        }

        private async Task OnTimer(object arg) {
            var creditChecksPassYet = await CheckCredit();
            if (creditChecksPassYet.HasValue) {
                _state.State.IsApproved = creditChecksPassYet.Value;
                await GrainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0).OnLoanApplicationProcessed(_state.State);
                _timerHandle.Dispose();
            }
        }

        public async Task Set(LoanApplication app) {
            _state.State = app;
            _state.State.Received = DateTime.Now;
            await _state.WriteStateAsync();
        }

        public async Task<LoanApplication> Get() {
            await _state.ReadStateAsync();
            return _state.State;
        }

        public async Task<bool?> CheckCredit() {
            var loan = (await GrainFactory.GetGrain<ILoanProcessOrchestratorGrain>(0).GetLoansInProgress())
                .FirstOrDefault(l => l.ApplicationId == this.GetPrimaryKey());

            var result = await Task.WhenAll(
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    loan.ApplicationId, Constants.EASY_CO).Validate(loan),
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    loan.ApplicationId, Constants.EXPRESS_CO).Validate(loan),
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    loan.ApplicationId, Constants.SLOW_AND_LOW_CO).Validate(loan)
                );

            bool? res =
               result.Any(r => !r.IsApproved.HasValue)
                   ? null
                   : result.All(r => r.IsApproved.HasValue && r.IsApproved.Value);

            return res;
        }
    }
}
