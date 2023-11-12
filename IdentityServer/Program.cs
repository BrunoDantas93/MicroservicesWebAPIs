using IdentityServer.Services;
using IdentityServer.Services.Authentication;
using MicroservicesHelpers.Services;
using MicroservicesHelpers.Models.Authentication;
using MicroservicesHelpers.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<AuthenticationConfiguration>(builder.Configuration.GetSection("Authentication"));

// Add services to the container.
MultiAuthService.Authentication(builder);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

SwaggerServices.SwaggerConfigs(builder, "IdentityServer", 2);

SingletonService.SingletonConfig(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
