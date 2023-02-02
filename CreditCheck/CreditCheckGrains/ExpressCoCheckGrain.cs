using Orleans.Runtime;

[CollectionAgeLimit(Minutes = 2)]
public class ExpressCoCheckGrain : Grain, ICreditCheckGrain
{
    private readonly ILogger<ExpressCoCheckGrain> _logger;
    private readonly IPersistentState<CreditCheck> _state;

    public ExpressCoCheckGrain(ILogger<ExpressCoCheckGrain> logger,
        [PersistentState("ExpressCoProcessedLoans")]
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
                Agency = Constants.EXPRESS_CO,
                ApplicationId = this.GetPrimaryKey(),
                Completed = DateTime.Now,
                IsApproved = app.LoanAmount < 15000
            };
        }

        return _state.State;
    }
}