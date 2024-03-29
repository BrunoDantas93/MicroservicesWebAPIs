﻿using IdentityServer.Helpers.Authentication;

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
