using IdentityServer.Helpers.Authentication;
using IdentityServer.Services.Authentication;
using MicroservicesHelpers;

namespace IdentityServer.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        //Helpers
        builder.Services.AddSingleton<UserAuthHelper>();
        builder.Services.AddSingleton<AppAuthHelper>();
    }
}
