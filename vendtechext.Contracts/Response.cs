namespace vendtechext.Contracts
{
    public class Response : IDisposable
    {
        //private static readonly Lazy<response> _instance = new Lazy<response>(() => new response());
        private readonly APIResponse _response;
        public Response()
        {
            _response = new APIResponse();
        }


        //public static response Instance => _instance.Value;

        public Response WithStatus(string status)
        {
            _response.status = status;
            return this;
        }
        public Response WithStatusCode(int statusCode)
        {
            _response.statusCode = statusCode;
            return this;
        }
        public Response WithDetail(string detail)
        {
            _response.detailed = detail;
            return this;
        }
        public Response WithMessage(string message)
        {
            _response.message = message;
            return this;
        }
        public Response WithType(object type)
        {
            _response.result = type;
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
