using Orleans.Runtime;

[CollectionAgeLimit(Minutes = 2)]
public abstract class AgencyGrainBase : Grain, ICreditCheckGrain
{
    private readonly ILogger<AgencyGrainBase> _logger;
    private readonly IPersistentState<CreditCheck> _state;

    public AgencyGrainBase(ILogger<AgencyGrainBase> logger,
        [PersistentState("CreditChecks")]
        IPersistentState<CreditCheck> state)
    {
        _logger = logger;
        _state = state;
    }

    public Task<CreditCheck> Validate(LoanApplication app)
    {
        throw new NotImplementedException();
    }
}