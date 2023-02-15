namespace ContosoLoans {
    public interface ILoanProcessOrchestratorGrainObserver : IGrainObserver {
        Task OnAfterLoanApplicationProcessed(LoanApplication app);
        Task OnAfterLoanProcessChecked(LoanApplication app);
    }
}
