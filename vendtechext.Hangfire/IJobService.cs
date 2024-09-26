namespace vendtechext.Hangfire
{
    public interface IJobService
    {
        void FireAndForegtJob();
        void RecurringJob();
        void DelayedJob();
        void ConitnuationJob();
    }
}