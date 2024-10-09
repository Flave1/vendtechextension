using vendtechext.Contracts;

namespace vendtechext.BLL.Common
{
    public class Response : IDisposable
    {
        //private static readonly Lazy<Response> _instance = new Lazy<Response>(() => new Response());
        private readonly APIResponse _response;
        public Response()
        {
              _response = new APIResponse();
        }


        //public static Response Instance => _instance.Value;

        public Response WithStatus(string status)
        {
            _response.Status = status;
            return this;
        }
        public Response WithStatusCode(int statusCode)
        {
            _response.StatusCode = statusCode;
            return this;
        }
        public Response WithDetail(string detail)
        {
            _response.Detailed = detail;
            return this;
        }
        public Response WithMessage(string message)
        {
            _response.Message = message;
            return this;
        }
        public Response WithType(object type)
        {
            _response.Result = type;
            return this;
        }
        public APIResponse GenerateResponse()
        {
            Dispose();
            return _response;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
