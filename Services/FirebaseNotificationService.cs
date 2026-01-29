using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using localshopyNew.Data;
using localshopyNew.Models;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{

    public class FirebaseNotificationService
    {
        private readonly AppDBContext _context;
        private static bool _firebaseInitialized = false;

        public FirebaseNotificationService(AppDBContext context)
        {
            _context = context;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (_firebaseInitialized) return;

            var firebasePath = Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json");
            if (!File.Exists(firebasePath))
            {
                throw new FileNotFoundException("Firebase service account JSON not found.", firebasePath);
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebasePath)
            });

            _firebaseInitialized = true;
            Console.WriteLine("Firebase initialized successfully.");
        }

        public async Task<bool> SaveTokenAsync(string emailId, string token, string role)
        {
            if (string.IsNullOrEmpty(emailId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
                return false;

            token = token.Trim();

            var existing = await _context.UserDevices.FirstOrDefaultAsync(x => x.EmailId == emailId && x.FcmToken == token);

            if (existing != null) return true;

            var oldTokens = await _context.UserDevices
                .Where(x => x.EmailId == emailId)
                .ToListAsync();

            _context.UserDevices.RemoveRange(oldTokens);

            _context.UserDevices.Add(new UserDevice
            {
                EmailId = emailId,
                Role = role,
                FcmToken = token
            });

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"FCM token saved for {emailId}: {token}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving FCM token: {ex.Message}");
                return false;
            }
        }

        public async Task SendNotificationAsync(string title, string body, string emailId)
        {
            var userDevice = await _context.UserDevices.FirstOrDefaultAsync(x => x.EmailId == emailId);
            if (userDevice == null) return;

            var message = new Message
            {
                Token = userDevice.FcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Notification sent: {response}");
            }
            catch (FirebaseMessagingException ex)
            {
                if (ex.ErrorCode.ToString() == "invalid-argument")
                {
                    Console.WriteLine($"Invalid token for {emailId}: {userDevice.FcmToken}, removing from DB");
                    _context.UserDevices.Remove(userDevice);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"FCM error: {ex.Message}");
                }
            }
        }
    }

}
