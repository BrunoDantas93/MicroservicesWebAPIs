using CommunicationService.Hubs;
using CommunicationService.Services;

namespace CommunicationService.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CommunicationHub>();
        builder.Services.AddSingleton<HubService>();
        builder.Services.AddSingleton<ChatService>();
    }
}
