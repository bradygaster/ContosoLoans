using Orleans.Runtime;

namespace ContosoLoans.LoanReception {
    public class LoanProcessOrchestratorGrain : Grain, ILoanProcessOrchestratorGrain {
        private readonly ILogger<LoanProcessOrchestratorGrain> _logger;
        private readonly IPersistentState<List<LoanApplication>> _state;
        private readonly HashSet<ILoanProcessOrchestratorGrainObserver> _observers = new();
        private Task? _outstandingWriteStateOperation;
        
        public LoanProcessOrchestratorGrain(ILogger<LoanProcessOrchestratorGrain> logger,
            [PersistentState("LoanApplicationsInProgress")]
            IPersistentState<List<LoanApplication>> state) {
            _logger = logger;
            _state = state;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken) {
            await _state.ReadStateAsync();
            RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken) {
            await _state.WriteStateAsync();
        }

        private async Task OnTimer(object state) {
            await this.AsReference<ILoanProcessOrchestratorGrain>().OnTimerTick();
        }

        public async Task OnTimerTick() {
            var tasks = new List<Task>();
            var loansToProcess = await GetLoansInProgress();
            foreach (var x in loansToProcess) {
                tasks.Add(ProcessLoan(x));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessLoan(LoanApplication loanApp) {
            _logger.LogInformation($"Checking status of loan application {loanApp.ApplicationId}.");

            var loanAppGrain = GrainFactory.GetGrain<ILoanApplicationGrain>(loanApp.ApplicationId);
            var creditChecksPassYet = await loanAppGrain.CheckCredit();

            if (creditChecksPassYet.HasValue) {
                loanApp.IsApproved = creditChecksPassYet.Value;
                loanApp.Processed = DateTime.Now.ToUniversalTime();
                await loanAppGrain.Set(loanApp);
                loanApp = await loanAppGrain.Get();
                await OnLoanApplicationProcessed(loanApp);
            }
            else {
                await loanAppGrain.Set(loanApp);
                loanApp = await loanAppGrain.Get();
                await OnLoanApplicationChecked(loanApp);
            }
        }

        public async Task<List<LoanApplication>> GetLoansInProgress() {
            var result = new List<LoanApplication>();

            foreach (var app in _state.State.OrderBy(_ => _.Received)) {
                var loanAppGrain = GrainFactory.GetGrain<ILoanApplicationGrain>(app.ApplicationId);
                var loanApp = await loanAppGrain.Get();
                result.Add(loanApp);
            }

            return result;
        }

        public async Task StartEvaluation(LoanApplication app) {
            _state.State.Add(app);
            await _state.WriteStateAsync();

            var loanAppGrain = GrainFactory.GetGrain<ILoanApplicationGrain>(app.ApplicationId);
            await loanAppGrain.Set(app);
        }

        public async Task OnLoanApplicationChecked(LoanApplication app) {
            _logger.LogInformation($"Loan application {app.ApplicationId} checked.");
            
            foreach (var observer in _observers) {
                if (observer != null) {
                    try {
                        await observer.OnAfterLoanProcessChecked(app);
                    }
                    catch {
                        _observers.Remove(observer);
                    }
                }
            }
        }

        public async Task OnLoanApplicationProcessed(LoanApplication app) {
            _state.State.RemoveAll(x => x.ApplicationId == app.ApplicationId);
            
            if (_outstandingWriteStateOperation is Task currentWriteStateOperation) {
                try {
                    await currentWriteStateOperation;
                }
                catch {
                }
                finally {
                    if (_outstandingWriteStateOperation == currentWriteStateOperation) {
                        _outstandingWriteStateOperation = null;
                    }
                }
            }

            if (_outstandingWriteStateOperation is null) {
                currentWriteStateOperation = _state.WriteStateAsync();
                _outstandingWriteStateOperation = currentWriteStateOperation;
            }
            else {
                currentWriteStateOperation = _outstandingWriteStateOperation;
            }

            try {
                await currentWriteStateOperation;
            }
            finally {
                if (_outstandingWriteStateOperation == currentWriteStateOperation) {
                    _outstandingWriteStateOperation = null;
                }
            }

            _logger.LogInformation($"Loan application {app.ApplicationId} processed. Result: {app.IsApproved}");

            foreach (var observer in _observers) {
                if (observer != null) {
                    try {
                        await observer.OnAfterLoanApplicationProcessed(app);
                    }
                    catch {
                        _observers.Remove(observer);
                    }
                }
            }
        }

        public Task Subscribe(ILoanProcessOrchestratorGrainObserver observer) {
            if (observer != null && !_observers.Contains(observer)) {
                _observers.Add(observer);
            }

            return Task.CompletedTask;
        }

        public Task Unsubscribe(ILoanProcessOrchestratorGrainObserver observer) {
            if (observer != null && _observers.Contains(observer)) {
                _observers.Remove(observer);
            }

            return Task.CompletedTask;
        }
    }
}
