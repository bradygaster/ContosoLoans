using Orleans.Runtime;

[CollectionAgeLimit(Minutes = 2)]
public class EasyCoCheckGrain : Grain, ICreditCheckGrain
{
    private readonly ILogger<EasyCoCheckGrain> _logger;
    private readonly IPersistentState<CreditCheck> _state;

    public EasyCoCheckGrain(ILogger<EasyCoCheckGrain> logger,
        [PersistentState("EasyCoProcessedLoans")]
            IPersistentState<CreditCheck> state)
    {
        _logger = logger;
        _state = state;
    }

    public async Task<CreditCheck> Validate(LoanApplication app)
    {
        if (!_state.RecordExists)
        {
            _state.State = new CreditCheck
            {
                Agency = Constants.EASY_CO,
                ApplicationId = this.GetPrimaryKey(),
                Completed = DateTime.Now,
                IsApproved = true
            };
        }

        return _state.State;
    }
}