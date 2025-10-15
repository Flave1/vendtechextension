using FirebaseAdmin.Messaging;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.Helper;
using vendtechext.SDK;

namespace vendtechext.BLL.Services
{
    public class MobilePushService : IMobilePushService
    {
        private readonly LogService _log;

        public MobilePushService(LogService log)
        {
            _log = log;
        }

        public async Task Push(MessageRequest request)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = request.Title,
                        Body = request.Body,
                    },
                    Data = new Dictionary<string, string>()
                    {
                        ["Priority"] = "high",
                        ["Type"] = request.NotificationType,
                        ["Sound"] = "default",
                        ["Id"] = request.Id

                    },
                    Token = request.DeviceToken
                };

                var messaging = FirebaseMessaging.DefaultInstance;
                var result = await messaging.SendAsync(message);

                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("message sent successfully!");
                }
                else
                {
                    Console.WriteLine("Error sending the message.");
                }
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                Console.WriteLine($"Reason: {ex.ErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public async Task Push(List<MessageRequest> requests)
        {
            try
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    var message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = requests[i].Title,
                            Body = requests[i].Body,
                        },
                        Data = new Dictionary<string, string>()
                        {
                            ["Priority"] = "high",
                            ["Type"] = requests[i].NotificationType,
                            ["Sound"] = "default",
                            ["Id"] = requests[i].Id

                        },
                        Token = requests[i].DeviceToken
                    };
                    try
                    {

                        var messaging = FirebaseMessaging.DefaultInstance;
                        var result = await messaging.SendAsync(message);
                        if (!string.IsNullOrEmpty(result))
                        {
                            _log.Log(LogType.Infor, $"push_notification_{requests[i].Id}", result);
                            Console.WriteLine("message sent successfully!");
                        }
                        else
                        {
                            Console.WriteLine("Error sending the message.");
                        }
                    }
                    catch (FirebaseMessagingException ex)
                    {
                        Console.WriteLine($"Error sending message: {ex.Message}");
                        _log.Log(LogType.Error, ex.Message, ex);
                        Console.WriteLine($"Reason: {ex.ErrorCode}");
                    }
                }
            }
           
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
