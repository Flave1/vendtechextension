
namespace vendtechext.BLL.Interfaces
{
    public interface IHttpRequestService
    {
        Task<HttpResponseMessage> SendPostAsync<T>(string requestUrl, T body);
    }
}
