using Orleans.Runtime;

[CollectionAgeLimit(Minutes = 2)]
public abstract class CreditCheckGrainBase : Grain, ICreditCheckGrain {
    protected readonly ILogger<CreditCheckGrainBase> _logger;
    protected readonly IPersistentState<CreditCheck> _state;

    public CreditCheckGrainBase(ILogger<CreditCheckGrainBase> logger,
        [PersistentState("CreditChecks")]
        IPersistentState<CreditCheck> state) {
        _logger = logger;
        _state = state;
    }

    public abstract Task<CreditCheck> Validate(LoanApplication app);

    public override async Task OnDeactivateAsync(DeactivationReason reason,
        CancellationToken cancellationToken) {
        await _state.WriteStateAsync();
    }
}