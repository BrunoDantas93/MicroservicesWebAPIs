using MicroservicesHelpers.Services;

namespace EventsAPI.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<EventsTypeService>();
        builder.Services.AddSingleton<EventsService>();
        builder.Services.AddSingleton<FirebasePushNotificationService>();
        builder.Services.AddSingleton<InstagramServices>();
    }
}
