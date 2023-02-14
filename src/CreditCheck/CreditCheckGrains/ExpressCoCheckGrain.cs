using Orleans.Runtime;

public class ExpressCoCheckGrain : CreditCheckGrainBase {
    public ExpressCoCheckGrain(ILogger<CreditCheckGrainBase> logger,
        [PersistentState("CreditChecks", null)]
        IPersistentState<CreditCheck> state) : base(logger, state) {
    }

    public override async Task<CreditCheck> Validate(LoanApplication app) {
        if (!_state.RecordExists) {
            _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.EXPRESS_CO} started");
            _state.State = new CreditCheck {
                Agency = Constants.EXPRESS_CO,
                ApplicationId = this.GetPrimaryKey(),
                Completed = DateTime.Now.ToUniversalTime(),
                IsApproved = app.LoanAmount < 15000
            };
            _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.EXPRESS_CO} completed");
            await _state.WriteStateAsync();
        }

        return _state.State;
    }
}