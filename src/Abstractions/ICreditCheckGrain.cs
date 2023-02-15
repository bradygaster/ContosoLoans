namespace ContosoLoans {
    public interface ICreditCheckGrain : IGrainWithGuidKey {
        Task<CreditCheck> Validate(LoanApplication app);
    }
}
