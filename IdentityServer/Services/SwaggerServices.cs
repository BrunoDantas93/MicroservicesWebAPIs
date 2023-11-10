using Microsoft.OpenApi.Models;

namespace IdentityServer.Services;

public class SwaggerServices
{
    public static void SwaggerConfigs(WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityServer", Version = "v1" });

            c.AddSecurityDefinition("UserAuth", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "User authentication token in the format 'Bearer <token>'",
                BearerFormat = "JWT",
            });

            c.AddSecurityDefinition("AppAuth", new OpenApiSecurityScheme
            {
                Name = "ApiKey",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = "Application authentication token",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "UserAuth"
                            }
                        },
                        new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "AppAuth"
                            }
                        },
                        new List<string>()
                    }
                });
        });

    }
}
