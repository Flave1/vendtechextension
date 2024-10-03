//using Hangfire;
//using vendtechext.Contracts;

//namespace vendtechext.Hangfire
//{
//    public class QueueJobs
//    {
//        private readonly IBackgroundJobClient _backgroundJobClient;
//        private readonly IRecurringJobManager _recurringJobManager;
//        public QueueJobs(IBackgroundJobClient backgroundJobClient,
//            IRecurringJobManager recurringJobManager)
//        {
//            _backgroundJobClient = backgroundJobClient;
//            _recurringJobManager = recurringJobManager;
//        }

//        public void CreateFireAndForget(string jobId)
//        {
//            //_backgroundJobClient.Enqueue(() => _jobService.FireAndForegtJob());
//        }
//        public void CreateDelayedJob(string jobId)
//        {
//            //_backgroundJobClient.Schedule(() => _jobService.DelayedJob(), TimeSpan.FromSeconds(60));
//        }

//        public void ContinuationJob(string jobId)
//        {
//            //var parentJobId = _backgroundJobClient.Enqueue(() => _jobService.FireAndForegtJob());
//            //_backgroundJobClient.ContinueJobWith(parentJobId, () => _jobService.ConitnuationJob());
//        }
//    }
//}
