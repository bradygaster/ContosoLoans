using Orleans.Runtime;

[GrainType(Constants.EASY_CO)]
public class EasyCoCheckGrain : CreditCheckGrainBase {
    public EasyCoCheckGrain(ILogger<CreditCheckGrainBase> logger,
        [PersistentState("CreditChecks", null)]
        IPersistentState<CreditCheck> state) : base(logger, state) {
    }

    public override async Task<CreditCheck> Validate(LoanApplication app) {
        if (!_state.RecordExists) {
            _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.EASY_CO} started");

            // simulate a longer running check
            await Task.Delay(Random.Shared.Next(1000, 3000));
            
            _state.State = new CreditCheck {
                Agency = Constants.EASY_CO,
                ApplicationId = this.GetPrimaryKey(),
                Completed = DateTime.Now.ToUniversalTime(),
                IsApproved = true
            };
            _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.EASY_CO} completed");
            await _state.WriteStateAsync();
        }

        return _state.State;
    }
}