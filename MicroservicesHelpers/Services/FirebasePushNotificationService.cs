using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;

namespace MicroservicesHelpers.Services;

public class FirebasePushNotificationService
{
    private readonly FirebaseMessaging _firebaseMessaging;

    public FirebasePushNotificationService()
    {
        try
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("../etc/key.json"),
            });

            _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Firebase initialization failed: {ex.Message}");
            throw; // Rethrow the exception for the calling code to handle
        }
    }

    public async Task SendPushNotificationAsync(string deviceToken, string title, string body)
    {
        try
        {
            // Construct the message payload
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Token = deviceToken
            };

            // Send the message
            var response = await _firebaseMessaging.SendAsync(message);
            Console.WriteLine($"Successfully sent message: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send push notification: {ex.Message}");
            throw; // Rethrow the exception for the calling code to handle
        }
    }
}
