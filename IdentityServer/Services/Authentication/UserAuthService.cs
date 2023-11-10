using MicroservicesHelpers.Models.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IdentityServer.Services.Authentication;

public class UserAuthService
{
    public static void UserAuth(WebApplicationBuilder builder)
    {
        AuthenticationConfiguration authConfig = builder.Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authConfig.Issuer,
                ValidAudience = authConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.UserAccessTokenSecret)),
                NameClaimType = "user_type" // Custom claim to differentiate user token
            };
            //options.Events = new JwtBearerEvents
            //{
            //    OnMessageReceived = context =>
            //    {
            //        var accessToken = context.Request.Query["access_token"];

            //        if (!string.IsNullOrEmpty(accessToken))
            //        {
            //            context.Token = accessToken;
            //        }

            //        return Task.CompletedTask;
            //    }
            //};
            // Modify the OnMessageReceived method to handle the differentiation
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    Debug.WriteLine("adsadasdas");
                    if (!context.Request.Headers.ContainsKey("ApiKey") && context.Request.Headers["Authorization"].ToString().StartsWith("Bearer"))
                    {
                        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                        if (jsonToken != null && jsonToken.Claims.Any(c => c.Type == null || (c.Type != null && c.Type != "user_type")))
                        {
                            // Reject any attempt to use the app token as a user token
                            context.Fail("Unauthorized");
                            context.Response.StatusCode = 401;
                        }
                        else
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                        }

                        // Allow the user token to be validated, as it contains the required "user_type" claim
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
            //    //OnMessageReceived = context =>
            //    //{
            //    //    if (!context.Request.Headers.ContainsKey("ApiKey") && context.Request.Headers["Authorization"].ToString().StartsWith("Bearer"))
            //    //    {
            //    //        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            //    //        var handler = new JwtSecurityTokenHandler();
            //    //        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            //    //        if (jsonToken != null && jsonToken.Claims.Any(c => c.Type == null || (c.Type != null && c.Type != "user_type")))
            //    //        {
            //    //            // Reject any attempt to use the app token as a user token
            //    //            context.Response.StatusCode = 401;
            //    //        }
            //    //    }

            //    //    return Task.CompletedTask;

            //    //    //// Check for the custom claim or header specific to the user token
            //    //    //if (!context.Request.Headers.ContainsKey("ApiKey") && !context.Token.Contains("user_token_claim"))
            //    //    //{
            //    //    //    // Reject any attempt to use the app token as a user token
            //    //    //    context.NoResult();
            //    //    //}

            //    //    //return Task.CompletedTask;
            //    //}
            //};
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
        }); ;        

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("UserPolicy", policy => policy.RequireClaim("user_type"));
        });
    }
}
