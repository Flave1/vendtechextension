using vendtechext.BLL.Common;

namespace vendtechext.BLL.Services
{
    public class BaseService
    {
        public BaseService()
        {
            Response = new Response();     
        }
        public Response Response { get;set; } = new Response();
    }
}
