using Orleans.Runtime;

namespace ContosoLoans.LoanReception {
    [CollectionAgeLimit(Minutes = 2)]
    public class LoanApplicationGrain : Grain, ILoanApplicationGrain {
        private readonly ILogger<LoanApplicationGrain> _logger;
        private readonly IPersistentState<LoanApplication> _state;
        private IDisposable _timerHandle;

        public LoanApplicationGrain(ILogger<LoanApplicationGrain> logger,
            [PersistentState("LoanApplication")]
            IPersistentState<LoanApplication> state) {
            _logger = logger;
            _state = state;
        }

        public async Task Set(LoanApplication app) {
            _state.State = app;
            _state.State.Received = _state.State.Received ?? DateTime.Now.ToUniversalTime();
            _state.State.LastUpdate = DateTime.Now.ToUniversalTime();
            await _state.WriteStateAsync();
        }

        public async Task<LoanApplication> Get() {
            await _state.ReadStateAsync();
            return _state.State;
        }

        public async Task<bool?> CheckCredit() {
            var result = await Task.WhenAll(
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    GrainId.Create(Constants.EASY_CO, _state.State.ApplicationId.ToString("N")))
                        .Validate(_state.State),
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    GrainId.Create(Constants.EXPRESS_CO, _state.State.ApplicationId.ToString("N")))
                        .Validate(_state.State),
                GrainFactory.GetGrain<ICreditCheckGrain>(
                    GrainId.Create(Constants.SLOW_AND_LOW_CO, _state.State.ApplicationId.ToString("N")))
                        .Validate(_state.State)
                );

            bool? res =
               result.Any(r => !r.IsApproved.HasValue)
                   ? null
                   : result.All(r => r.IsApproved.HasValue && r.IsApproved.Value);

            return res;
        }
    }
}
