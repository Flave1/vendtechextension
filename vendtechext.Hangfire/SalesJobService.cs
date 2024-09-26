using Microsoft.Extensions.Logging;

namespace vendtechext.Hangfire
{
    public class SalesJobService : IJobService
    {
        private readonly ILogger _logger;

        public SalesJobService(ILogger<SalesJobService> logger)
        {
            _logger = logger;
        }

        public void ConitnuationJob()
        {
            _logger.LogInformation("ConitnuationJob");
        }

        public void DelayedJob()
        {
            _logger.LogInformation("ConitnuationJob");
        }

        public void FireAndForegtJob()
        {
            _logger.LogInformation("ConitnuationJob");
        }

        public void RecurringJob()
        {
            _logger.LogInformation("ConitnuationJob");
        }
    }
}
