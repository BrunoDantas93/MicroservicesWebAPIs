using Microsoft.AspNetCore.SignalR;
using CommunicationService.Models;
using MicroservicesHelpers.Models.Authentication;
using IdentityServer.Services.Authentication;
using MicroservicesHelpers.Services;
using CommunicationService.Services;
using CommunicationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ConnectionConfigurations>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<AuthenticationConfiguration>(builder.Configuration.GetSection("Authentication"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.
MultiAuthService.Authentication(builder);

SwaggerServices.SwaggerConfigs(builder, "Communication Service", 1);


// Set up SignalR services to be used in the application
builder.Services.AddSignalR(o => 
{ 
    o.MaximumReceiveMessageSize = 102400000; 
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseCors("AllowAll");

app.MapHub<CommunicationHub>("/hub");

app.MapControllers();

app.Run();
