namespace ContosoLoans {
    public interface ILoanProcessOrchestratorGrain : IGrainWithIntegerKey {
        Task StartEvaluation(LoanApplication app);
        Task OnLoanApplicationProcessed(LoanApplication app);
        Task OnLoanApplicationChecked(LoanApplication app);
        Task<List<LoanApplication>> GetLoansInProgress();
        Task Subscribe(ILoanProcessOrchestratorGrainObserver observer);
        Task Unsubscribe(ILoanProcessOrchestratorGrainObserver observer);
    }
}
