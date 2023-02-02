using Orleans.Runtime;

public class ExpressCoCheckGrain : CreditCheckGrainBase
{
    public ExpressCoCheckGrain(ILogger<CreditCheckGrainBase> logger,
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
                Agency = Constants.EXPRESS_CO,
                ApplicationId = this.GetPrimaryKey(),
                Completed = DateTime.Now,
                IsApproved = app.LoanAmount < 15000
            };
        }

        return _state.State;
    }
}