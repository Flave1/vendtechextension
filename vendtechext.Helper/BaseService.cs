using vendtechext.Contracts;

namespace vendtechext.Helper
{
    public class BaseService
    {
        public BaseService()
        {
            Response = new Response();
        }
        public Response Response { get; set; } = new Response();
    }
}
