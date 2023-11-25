using IdentityServer.Helpers.Authentication;
using IdentityServer.Services.Authentication;
using MicroservicesHelpers.Models;
using MicroservicesHelpers.Services;

namespace IdentityServer.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        //Helpers
        builder.Services.AddSingleton<UserAuthHelper>();
        builder.Services.AddSingleton<AppAuthHelper>();

        builder.Services.AddSingleton<UserService>();
    }
}
