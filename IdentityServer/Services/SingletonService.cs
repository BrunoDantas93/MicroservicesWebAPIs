using IdentityServer.Helpers.Authentication;
using IdentityServer.Services.Authentication;

namespace IdentityServer.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        //Helpers
        builder.Services.AddSingleton<UserAuthHelper>();
        builder.Services.AddSingleton<AppAuthHelper>();


        //Services
        builder.Services.AddSingleton<UserAuthService>();
        builder.Services.AddSingleton<AppAuthService>();

    }
}
