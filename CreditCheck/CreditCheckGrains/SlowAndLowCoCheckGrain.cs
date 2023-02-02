using Orleans.Runtime;

public class SlowAndLowCoCheckGrain : CreditCheckGrainBase
{
    private readonly IPersistentState<List<CreditCheck>> _checksInProgress;
    private IDisposable _timerHandle;

    public SlowAndLowCoCheckGrain(ILogger<CreditCheckGrainBase> logger, 
        [PersistentState("CreditChecks", null)] IPersistentState<CreditCheck> state,
        [PersistentState("CreditChecksInProgress", null)] IPersistentState<List<CreditCheck>> checksInProgress) : base(logger, state)
    {
        _checksInProgress = checksInProgress;
    }

    public override async Task<CreditCheck> Validate(LoanApplication app)
    {
        var isThisInProcessAlready =
            _checksInProgress.State.Any(x
                => x.ApplicationId == app.ApplicationId);

        if (!isThisInProcessAlready && !_state.RecordExists)
        {
            _state.State = new CreditCheck
            {
                Agency = Constants.SLOW_AND_LOW_CO,
                ApplicationId = app.ApplicationId,
                Started = DateTime.Now,
                Completed = null,
                IsApproved = null
            };
            _checksInProgress.State.Add(_state.State);
        }
        else
        {
            if (_state.State.Started.ToUniversalTime().AddSeconds(30)
                < DateTime.Now.ToUniversalTime())
            {
                _state.State.IsApproved = app.LoanAmount < 5000;
                _state.State.Completed = DateTime.Now;
                _checksInProgress.State.RemoveAll(x => x.ApplicationId == app.ApplicationId);
                _timerHandle?.Dispose();
            }
        }

        return _state.State;
    }
}
