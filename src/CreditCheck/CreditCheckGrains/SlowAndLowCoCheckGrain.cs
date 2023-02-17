using Orleans.Runtime;

[GrainType(Constants.SLOW_AND_LOW_CO)]
public class SlowAndLowCoCheckGrain : CreditCheckGrainBase {
    private readonly IPersistentState<List<CreditCheck>> _checksInProgress;

    public SlowAndLowCoCheckGrain(ILogger<CreditCheckGrainBase> logger,
        [PersistentState("CreditChecks", null)] IPersistentState<CreditCheck> state,
        [PersistentState("CreditChecksInProgress", null)] IPersistentState<List<CreditCheck>> checksInProgress) : base(logger, state) {
        _checksInProgress = checksInProgress;
    }

    public override async Task<CreditCheck> Validate(LoanApplication app) {
        var isThisInProcessAlready =
            _checksInProgress.State.Any(x
                => x.ApplicationId == app.ApplicationId);

        if (!isThisInProcessAlready && !_state.RecordExists) {
            _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.SLOW_AND_LOW_CO} started");
            _state.State = new CreditCheck {
                Agency = Constants.SLOW_AND_LOW_CO,
                ApplicationId = app.ApplicationId,
                Started = DateTime.Now.ToUniversalTime(),
                Completed = null,
                IsApproved = null
            };
            _checksInProgress.State.Add(_state.State);
        }
        else {
            var nxt = Random.Shared.Next(30, 90);
            if (_state.State.Started.ToUniversalTime().AddSeconds(nxt)
                < DateTime.Now.ToUniversalTime()) {
                _state.State.IsApproved = app.LoanAmount < 5000;
                _state.State.Completed = DateTime.Now;
                _checksInProgress.State.RemoveAll(x => x.ApplicationId == app.ApplicationId);
                _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.SLOW_AND_LOW_CO} completed");
            }
            else {
                _logger.LogInformation($"Credit check for {app.ApplicationId} with {Constants.SLOW_AND_LOW_CO} still in progress");
            }
        }


        await _checksInProgress.WriteStateAsync();
        await _state.WriteStateAsync();

        return _state.State;
    }
}
