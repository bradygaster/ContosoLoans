using Orleans.Runtime;

public class SlowAndLowCoCheckGrain : Grain, ICreditCheckGrain
{
    private readonly ILogger<SlowAndLowCoCheckGrain> _logger;
    private readonly IPersistentState<List<CreditCheck>> _checksInProgress;
    private readonly IPersistentState<CreditCheck> _state;
    private IDisposable _timerHandle;

    public SlowAndLowCoCheckGrain(
        ILogger<SlowAndLowCoCheckGrain> logger,
        [PersistentState("SlowAndLowCoLoansInProcess")]
            IPersistentState<List<CreditCheck>> checksInProgress,
        [PersistentState("SlowAndLowCoProcessedLoans")]
            IPersistentState<CreditCheck> state)
    {
        _logger = logger;
        _checksInProgress = checksInProgress;
        _state = state;
    }

    public async Task<CreditCheck> Validate(LoanApplication app)
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
