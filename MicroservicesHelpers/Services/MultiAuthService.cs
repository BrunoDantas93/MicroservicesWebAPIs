using MicroservicesHelpers.Models.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer.Services.Authentication;

public class MultiAuthService
{
    public static void Authentication(WebApplicationBuilder builder)
    {
        AuthenticationConfiguration authConfig = builder.Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "MultiAuthSchemes";
            options.DefaultChallengeScheme = "MultiAuthSchemes";
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authConfig.Issuer,
                ValidAudience = authConfig.Audience,
                IssuerSigningKey = GetRsaSecurityKey(),
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.UserPrivateKey)),//GetRsaSecurityKey(),//new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.UserPrivateKey)),
                NameClaimType = "user_type" // Custom claim to differentiate user token
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer("SecondJwtScheme", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authConfig.Issuer,
                ValidAudience = authConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.AppPrivateKey)),
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
        })
        .AddPolicyScheme("MultiAuthSchemes", JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization];
                var apiKey = context.Request.Headers.ContainsKey("ApiKey");

                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    if (!context.Request.Headers.ContainsKey("ApiKey") && context.Request.Headers["Authorization"].ToString().StartsWith("Bearer"))
                    {
                        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                        if (jsonToken != null && jsonToken.Claims.Any(c => c.Type == null || (c.Type != null && c.Type == "user_type")))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                    }
                }
                else if (apiKey)
                {
                    return "SecondJwtScheme";
                }

                return JwtBearerDefaults.AuthenticationScheme;
            };
        });
    }


    private static RsaSecurityKey GetRsaSecurityKey()
    {
        // Load the RSA public key
        var rsa = RSA.Create();

        var publicKeyText = File.ReadAllText("../etc/RSA/PEM/public_key.pem");
        rsa.ImportFromPem(publicKeyText);

        return new RsaSecurityKey(rsa);
    }
}
