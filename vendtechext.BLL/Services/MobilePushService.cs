﻿using FirebaseAdmin.Messaging;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;

namespace vendtechext.BLL.Services
{
    public class MobilePushService : IMobilePushService
    {
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
                    Console.WriteLine("Message sent successfully!");
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
    }
}
