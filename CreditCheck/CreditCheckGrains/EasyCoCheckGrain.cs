using Orleans.Runtime;

public class EasyCoCheckGrain : CreditCheckGrainBase
{
    public EasyCoCheckGrain(ILogger<CreditCheckGrainBase> logger, 
        [PersistentState("CreditChecks", null)] 
        IPersistentState<CreditCheck> state) : base(logger, state)
    {
    }

    public override async Task<CreditCheck> Validate(LoanApplication app)
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