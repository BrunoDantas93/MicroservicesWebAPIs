using CommunicationService.Models.MongoDB;
using CommunicationService.Models.Requests;
using CommunicationService.Services;
using FirebaseAdmin.Messaging;
using MicroservicesHelpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CommunicationService.Helpers.Enumerated;
using static MicroservicesHelpers.Enumerated;

namespace CommunicationService.Controllers;

/// <summary>
/// Controller responsible for handling API requests related to chat operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    /// <summary>
    /// The logger for logging information.
    /// </summary>
    private readonly ILogger _logger;

    
    private readonly HubService _hubService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatController"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging information.</param>
    /// <param name="chatService">The service for managing chat-related functionalities.</param>
    public ChatController(ILogger<ChatController> logger, ChatService chatService, HubService hubService)
    {
        _logger = logger;
        _hubService = hubService;
    }

    //Post /
    /// <summary>
    /// Creates a new chat.
    /// </summary>
    /// <remarks>
    /// This endpoint requires authentication.
    /// 
    ///     /Post
    ///     {
    ///       "type": 0,
    ///       "name": "string",
    ///       "participants": [
    ///         "string"
    ///       ],
    ///       "messages": [
    ///         {
    ///           "content": "string",
    ///           "senderId": "string",
    ///           "receiverId": "string"
    ///         }
    ///       ]
    ///     }
    /// </remarks>
    /// <param name="chat">The chat object containing the necessary information.</param>
    /// <returns>Returns an IActionResult indicating the result of the chat creation.</returns>
    /// <response code="200">OK - The chat was created successfully.</response>
    /// <response code="400">Bad Request - Indicates issues or validation errors during chat creation.</response>
    /// <response code="401">Unauthorized - The user is not authenticated to create a chat.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost, Authorize]
    public async Task<IActionResult> CreateChat([FromBody] ChatRequests chat)
    {
        try
        {
            // Attempt to create the chat
            MicroservicesResponse res = await _hubService.CreateChat(chat);

            // Return a successful response
            return Ok(res);
        }
        catch (Exception ex)
        {
            // Log the error for debugging and monitoring purposes
            _logger.LogError(ex, "Error creating a new chat.");

            // If an exception occurs during chat creation, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Error", "Error creating chat", ex.Message));
        }
    }

    //Put /AddMessage/{chatId}
    /// <summary>
    /// Adds a message to the specified chat.
    /// </summary>
    /// <remarks>
    /// This endpoint requires authentication.
    /// {
    ///   "content": "string",
    ///   "senderId": "string",
    ///   "receiverId": "string"
    /// }
    /// </remarks>
    /// <param name="chatId">The ID of the chat to which the message will be added.</param>
    /// <param name="message">The message object containing the content and details.</param>
    /// <returns>Returns an IActionResult indicating the result of adding the message to the chat.</returns>
    /// <response code="200">OK - The message was successfully added to the chat.</response>
    /// <response code="400">Bad Request - Indicates issues or validation errors during message addition.</response>
    /// <response code="401">Unauthorized - The user is not authenticated to add a message to the chat.</response>
    /// <response code="404">Not Found - The specified chat was not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("AddMessage/{chatId}"), Authorize]
    public async Task<IActionResult> AddMessageToChat(string chatId, [FromBody] MessageRequests message)
    {
        try
        {
            // Attempt to add the message to the chat
            var updatedChat = await _hubService.AddMessage(chatId, message);

            // Return a successful response
            return Ok(updatedChat);
        }
        catch (InvalidOperationException ex)
        {
            // Log the error and return a Not Found response with details
            _logger.LogError(ex.Message);
            return NotFound(new MicroservicesResponse(MicroservicesCode.Validation, "Chat Not Found", $"The chat with ID '{chatId}' was not found.", null));
        }
        catch (Exception ex)
        {
            // Log the error and return a BadRequest response with details
            _logger.LogError(ex,  $"Error  adding a message to the chat. ChatId: {chatId}");
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Error", "Error adding message to chat", null));
        }
    }

    //Patch /UpdateMessageStatus/{chatId}/{messageId}
    /// <summary>
    /// Updates the status of a message in the specified chat.
    /// </summary>
    /// <remarks>
    /// This endpoint requires authentication.
    ///     MessageStatus   'Sent', 'Delivered', 'Read'
    /// </remarks>
    /// <param name="chatId">The ID of the chat containing the message.</param>
    /// <param name="messageId">The ID of the message to update.</param>
    /// <param name="newStatus">The new status to set for the message.</param>
    /// <returns>Returns an IActionResult indicating the result of updating the message status.</returns>
    /// <response code="200">OK - The message status was successfully updated.</response>
    /// <response code="400">Bad Request - Indicates issues or validation errors during the status update.</response>
    /// <response code="401">Unauthorized - The user is not authenticated to update the message status.</response>
    /// <response code="404">Not Found - The specified chat or message was not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPatch("UpdateMessageStatus/{chatId}/{messageId}"), Authorize]
    public async Task<IActionResult> UpdateMessageStatus(string chatId, string messageId, [FromBody] MessageStatus newStatus)
    {
        try
        {
            // Attempt to update the message status
            var updatedChat = await _hubService.UpdateMessageStatus(chatId, messageId, newStatus);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Status Updated", "Message status updated successfully", updatedChat));
        }
        catch (InvalidOperationException ex)
        {
            // Log the error and return a Not Found response with details
            _logger.LogError(ex.Message);
            return NotFound(new MicroservicesResponse(MicroservicesCode.Validation, "Resource Not Found", "The requested chat or message was not found.", null));
        }
        catch (Exception ex)
        {
            // Log the error and return a BadRequest response with details
            _logger.LogError(ex, $"Error updating the status of a message from chat. ChatId: {chatId}, with MessageId: {messageId}");
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Error", "Error updating message status", null));
        }
    }





}
