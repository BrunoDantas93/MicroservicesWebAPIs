using CommunicationService.Models.SignalR;
using CommunicationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace CommunicationService.Hubs;

public class CommunicationHub : Hub
{

    private readonly ILogger _logger;
    public readonly HubService _hubService;

    public CommunicationHub(ILogger<CommunicationHub> logger, HubService hubService)
    {
        _logger = logger;
        _hubService = hubService;
    }

    /// <summary>
    /// Handles the connection event when a client connects to the hub.
    /// </summary>
    /// <returns>Asynchronous task.</returns>
    /// <exception cref="HubException">Exception thrown in case of connection failure.</exception>
    [Authorize]
    public override Task OnConnectedAsync()
    {
        try
        {
            // Create an instance of the Connection class to represent the user's connection
            Connection connection = new Connection();

            // Get user information from the token claims
            connection.UserID = Context.User.Claims.FirstOrDefault(c => c.Type == "userID")?.Value;
            connection.UserName = Context.User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            connection.Language = Context.User.Claims.FirstOrDefault(c => c.Type == "language")?.Value;

            // Check if the UserID is valid
            if (connection.UserID == null || string.IsNullOrEmpty(connection.UserID))
                throw new HubException("Invalid UserID. Connection refused.");

            // Call the HubService to create the connection in the backend
            _hubService.CreateConnection(connection, Context.ConnectionId);

            // Call the base method to continue with the default processing
            return base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            // Log any exception and rethrow it
            _logger.LogError(400, "Failed to create connection.", ex);
            throw;
        }
    }

    /// <summary>
    /// Handles the disconnection event when a client disconnects from the hub.
    /// </summary>
    /// <param name="exception">The exception that led to the disconnection, if any.</param>
    /// <returns>Asynchronous task.</returns>
    public override Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            // Call the HubService to disconnect the connection associated with the provided ConnectionId
            _hubService.DisconnectConnection(Context.ConnectionId);

            // Call the base method to continue with the default processing
            return base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            // Log any exception that occurs during disconnection and rethrow it
            _logger.LogError(400, "Failed to disconnect connection.", ex);
            throw;
        }
    }


    [Authorize]
    public async Task SendMessage()
    {
        await Clients.All.SendAsync("ReceiveMessage");
    }
}