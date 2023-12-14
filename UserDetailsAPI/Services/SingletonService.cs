using MicroservicesHelpers.Services;

namespace UserDetailsAPI.Services;

public class SingletonService
{
    public static void SingletonConfig(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<UsersDetailsService>();
        builder.Services.AddSingleton<ProfilePictureService>();
    }
}
