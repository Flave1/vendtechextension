namespace vendtechext.Contracts
{
    public class Response<T> : IDisposable
    {
        //private static readonly Lazy<response> _instance = new Lazy<response>(() => new response());
        private readonly APIResponse _response;
        public Response()
        {
            _response = new APIResponse();
        }

        //public static response Instance => _instance.Value;

        public Response<T> WithStatus(string status)
        {
            _response.status = status;
            return this;
        }
        public Response<T> WithDetail(string detail)
        {
            _response.detailed = detail;
            return this;
        }
        public Response<T> WithMessage(string message)
        {
            _response.message = message;
            return this;
        }
        public Response<T> WithType(T result)
        {
            _response.result = result;
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
