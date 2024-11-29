using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IMobilePushService
    {
        Task Push(MessageRequest request);
    }
}
