using MicroservicesHelpers.Models.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IdentityServer.Services.Authentication;

public class AppAuthService
{
    public static void AppAuth(WebApplicationBuilder builder)
    {
        AuthenticationConfiguration authConfig = builder.Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

        builder.Services.AddAuthentication("AppAuth")
        .AddJwtBearer("AppAuth", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authConfig.Issuer,
                ValidAudience = authConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.AppAccessTokenSecret)),
                NameClaimType = "application_type" // Custom claim to differentiate application token
            };
            
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.SecurityToken is JwtSecurityToken accessToken && accessToken.Payload.ContainsKey("application_type"))
                    {
                        // Allow the API token to be validated, as it contains the required "application_type" claim
                        return Task.CompletedTask;
                    }
                    else
                    {
                        // Reject the token if it doesn't contain the required claim
                        context.Fail("Unauthorized");
                        return Task.CompletedTask;
                    }
                }
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApplicationPolicy", policy => policy.RequireClaim("application_type"));
        });
    }
}
