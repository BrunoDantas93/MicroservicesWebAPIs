using EventsAPI.Models.MongoDB;
using EventsAPI.Services;
using IdentityServer.Models.Requests.User;
using MicroservicesHelpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MicroservicesHelpers.Enumerated;

namespace EventsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsTypeController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly EventsTypeService _eventsType;

    public EventsTypeController(ILogger<EventsTypeController> logger, EventsTypeService eventsType)
    {
        _logger = logger;
        _eventsType = eventsType;
    }

    // Get /
    /// <summary>
    /// Endpoint for listing event types.
    /// </summary>
    /// Example: 
    /// 
    ///     Get 
    ///     "type"
    ///     
    /// </remarks>
    /// <returns>An ActionResult representing the HTTP response for listing event types.</returns>
    /// <response code="200">OK - The list of event types was successfully retrieved.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the listing.</response>
    [HttpGet("", Name = "ListEventTypes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<EventsType>>> ListEventTypes([FromQuery] string? type = null)
    {
        try
        {
            // Attempt to retrieve a list of event types using the ListEventsTypes method
            // This method is assumed to return a Task<List<EventsType>>
            return Ok(await _eventsType.ListEventsTypes(type));
        }
        catch (Exception ex)
        {
            _logger.LogError(400, "Ocorreu um erro durante a listagem.", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante a listagem dos tipos envento.", ex.Message));
        }
    }

    //Post /
    /// <summary>
    /// Registers a new event type.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST 
    ///     {
    ///       "type": "string",
    ///       "isAgeRestriction": true
    ///     }
    ///     
    /// </remarks>
    /// <param name="type">The request payload containing information about the event type.</param>
    /// <returns>Returns an ActionResult indicating the result of the registration.</returns>
    /// <response code="200">OK - The event type was registered successfully.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the registration.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    /// <response code="403">Forbidden - The user does not have the necessary permissions (Admin role required).</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("", Name = "RegisterType"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> RegisterEventType([FromBody] EventsTypeRequest type)
    {
        try
        {
            List<EventsType> events = await _eventsType.ListEventsTypes(type: type.Type);

            if (events.Count > 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Tipo de evento já existe", "O tipo de evento especificado já existe. Por favor, use um tipo diferente.", null));

            EventsType eventsType = new EventsType();
            eventsType.Type = type.Type;
            eventsType.IsAgeRestriction = type.IsAgeRestriction;

            _eventsType.CreateEventType(eventsType);

            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Tipo de evento registrado com sucesso", "O tipo de evento especificado foi registrado com sucesso.", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(400, "Ocorreu um erro durante o registro.", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante o registo do tipo de envento.", ex.Message));
        }
    }

    //Put /{id}
    /// <summary>
    /// Updates the disabled status of an event type based on the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event type to update.</param>
    /// <returns>Returns an ActionResult indicating the result of the status update.</returns>
    /// <response code="200">OK - The disabled status of the event type was successfully updated.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the status update.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    /// <response code="403">Forbidden - The user does not have the necessary permissions (Admin role required).</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPut("{id}", Name = "ChangeDisabled"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> ChangeDisabled(string id)
    {
        try
        {
            // Retrieve the event type based on the specified identifier
            List<EventsType> events = await _eventsType.ListEventsTypes(uid: id);

            if (events.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "EventTypeNotFound", "The specified event type was not found.", null));

            // Call the method to update the disabled status of the event type
            await _eventsType.ChangeEventType(id);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "DisabledStatusChangedSuccessfully", "The disabled status of the event type was changed successfully.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "An error occurred during the modification of the disabled status.", ex);

            // If an exception occurs during the modification, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "FatalError", "An error occurred during the modification of the disabled status of the event type.", ex.Message));
        }
    }

}
