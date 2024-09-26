using Hangfire;

namespace vendtechext.Hangfire
{
    public class RetrieveJobs
    {
        private readonly IJobService _jobService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        public RetrieveJobs(IJobService jobService, IBackgroundJobClient backgroundJobClient, 
            IRecurringJobManager recurringJobManager)
        {
            _jobService = jobService;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        public void CreateFireAndForget(string transactionId, string title, string amount)
        {
            _backgroundJobClient.Enqueue(() => _jobService.FireAndForegtJob());
        }
        public void CreateDelayedJob(string transactionId, string title, string amount)
        {
            _backgroundJobClient.Schedule(() => _jobService.DelayedJob(), TimeSpan.FromSeconds(60));
        }
       
        public void CreateRecurringJob(string transactionId, string title, string amount)
        {
            _recurringJobManager.AddOrUpdate(transactionId, () => _jobService.RecurringJob(), Cron.Minutely);
        }

        public void ContinuationJob(string transactionId, string title, string amount)
        {
            var parentJobId = _backgroundJobClient.Enqueue(() => _jobService.FireAndForegtJob());
            _backgroundJobClient.ContinueJobWith(parentJobId, () => _jobService.ConitnuationJob());
        }
    }
}
