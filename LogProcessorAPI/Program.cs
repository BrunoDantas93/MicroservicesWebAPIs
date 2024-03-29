using IdentityServer.Services.Authentication;
using LogProcessorAPI.Helpers;
using LogProcessorAPI.Models;
using LogProcessorAPI.Services;
using MicroservicesHelpers.Models.Authentication;
using MicroservicesHelpers.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.Configure<ConnectionConfigurations>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<Logs>(builder.Configuration.GetSection("Logs"));
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

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

SwaggerServices.SwaggerConfigs(builder, "LogProcessor", 1);

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

app.MapControllers();

// Get an instance of the LogProcessor class from the DI container
var logProcessor = app.Services.GetService<LogProcessor>();

// Call the method to process logs
logProcessor.ProcessarLogs();

app.Run();
