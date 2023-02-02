namespace ContosoLoans
{
    public interface ILoanProcessOrchestratorGrain : IGrainWithIntegerKey
    {
        Task StartEvaluation(LoanApplication app);
        Task OnLoanApplicationProcessed(LoanApplication app);
        Task<List<LoanApplication>> GetLoansInProgress();
    }

    public interface ILoanApplicationGrain : IGrainWithGuidKey, IDisposable
    {
        Task Set(LoanApplication app);
        Task<bool?> CheckCredit();
    }

    public interface ICreditCheckGrain : IGrainWithGuidKey
    {
        Task<CreditCheck> Validate(LoanApplication app);
    }
    
    [GenerateSerializer]
    public class LoanApplication
    {
        [Id(0)]
        public Guid ApplicationId { get; set; } = Guid.NewGuid();
        [Id(1)]
        public Guid CustomerId { get; set; } = Guid.NewGuid();
        [Id(2)]
        public bool? IsApproved { get; set; }
        [Id(3)]
        public double LoanAmount { get; set; } = 0;
        [Id(4)]
        public DateTime? Received { get; set; }
        [Id(5)]
        public DateTime? Processed { get; set; }
    }

    [GenerateSerializer]
    public class CreditCheck
    {
        [Id(0)]
        public Guid ApplicationId { get; set; }
        [Id(1)]
        public string? Agency { get; set; }
        [Id(2)]
        public bool? IsApproved { get; set; } = false;
        [Id(3)]
        public DateTime? Completed { get; set; }
        [Id(4)]
        public DateTime Started { get; set; }
    }
}