using Hangfire;
using Moq;
using vendtechext.Hangfire;
using Xunit;

namespace vendtechext.TEST.Account
{
    public class JobsTests
    {
        private readonly Mock<IJobService> _serviceMoq;
        private readonly Mock<IBackgroundJobClient> _backgroundJobClientMoq;
        private readonly Mock<IRecurringJobManager> _recurringJobManagerMoq;
        private readonly RetrieveJobs _retrieveJobs;

        public JobsTests()
        {
            _serviceMoq = new Mock<IJobService>();
            _backgroundJobClientMoq = new Mock<IBackgroundJobClient>();
            _recurringJobManagerMoq = new Mock<IRecurringJobManager>();
            _retrieveJobs = new RetrieveJobs(_serviceMoq.Object, _backgroundJobClientMoq.Object, _recurringJobManagerMoq.Object);
        }

        [Theory]
        [InlineData("12345", "Sale", "12345678")]
        public void Create_Fire_And_Forget(string transactionId, string title, string amount)
        {
            // Arrange
            var requestBody = new 
            {
                transactionId,
                title,
                amount
            };
            // Act

            _retrieveJobs.CreateFireAndForget(transactionId, title, amount);

            // Assert
            _serviceMoq.Verify(es => es.FireAndForegtJob(), Times.Never);
        }
    }

}