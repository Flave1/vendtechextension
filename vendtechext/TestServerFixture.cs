using Microsoft.AspNetCore.Mvc.Testing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("vendtechext.TEST")]
namespace vendtechext
{

    internal class TestServerFixture : IDisposable
    {
        public WebApplicationFactory<Program> Factory { get; }
        public HttpClient Client { get; }

        public TestServerFixture()
        {
            Factory = new WebApplicationFactory<Program>();
            Client = Factory.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }
    }
}
