namespace ContosoLoans {
    public interface ILoanApplicationGrain : IGrainWithGuidKey {
        Task<LoanApplication> Get();
        Task Set(LoanApplication app);
        Task<bool?> CheckCredit();
    }
}
