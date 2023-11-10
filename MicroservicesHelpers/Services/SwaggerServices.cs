using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MicroservicesHelpers.Services;

public class SwaggerServices
{
    public static void SwaggerConfigs(WebApplicationBuilder builder, string name, int version)
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = name, Version = $"v{version}" });

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
