using FirebaseAdmin.Messaging;

namespace localshopyNew.Services
{
    public class FirebaseNotificationService
    {
        public async Task SendNotificationAsync(string fcmToken, string title, string body)
        {
            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }

}