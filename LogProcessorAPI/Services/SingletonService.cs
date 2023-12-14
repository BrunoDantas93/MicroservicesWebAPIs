
using LogProcessorAPI.Helpers;

namespace LogProcessorAPI.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        //Helpers
        builder.Services.AddSingleton<LogProcessor>();
        builder.Services.AddSingleton<LogService>();
    }
}
