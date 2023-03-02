using Orleans.Runtime;
using Orleans.Utilities;

namespace ContosoLoans.LoanReception {
    public class LoanProcessOrchestratorGrain : Grain, ILoanProcessOrchestratorGrain {
        private readonly ILogger<LoanProcessOrchestratorGrain> _logger;
        private readonly IPersistentState<List<LoanApplication>> _state;
        private readonly ObserverManager<ILoanProcessOrchestratorGrainObserver> _subsManager;
        private Task? _outstandingWriteStateOperation;

        public LoanProcessOrchestratorGrain(ILogger<LoanProcessOrchestratorGrain> logger,
            [PersistentState("LoanApplicationsInProgress")]
            IPersistentState<List<LoanApplication>> state) {
            _logger = logger;
            _state = state;
            _subsManager = new ObserverManager<ILoanProcessOrchestratorGrainObserver>(
                TimeSpan.FromMinutes(5), logger); ;
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

        public Task Subscribe(ILoanProcessOrchestratorGrainObserver observer) {
            _subsManager.Subscribe(observer, observer);
            return Task.CompletedTask;
        }

        public Task Unsubscribe(ILoanProcessOrchestratorGrainObserver observer) {
            _subsManager.Unsubscribe(observer);
            return Task.CompletedTask;
        }

        public async Task OnLoanApplicationChecked(LoanApplication app) {
            _logger.LogInformation($"Loan application {app.ApplicationId} checked.");
            await _subsManager.Notify(async _ => await _.OnAfterLoanProcessChecked(app));
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
            await _subsManager.Notify(async _ => await _.OnAfterLoanApplicationProcessed(app));
        }
    }
}
