using EventsAPI.Models.MongoDB;
using EventsAPI.Models.Requests;
using EventsAPI.Services;
using MicroservicesHelpers;
using MicroservicesHelpers.Models;
using MicroservicesHelpers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Security.Claims;
using static EventsAPI.Helpers.Enumerated;
using static MicroservicesHelpers.Enumerated;

namespace EventsAPI.Controllers;


[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly EventsTypeService _eventsTypeService;
    private readonly EventsService _eventService;
    private readonly MailSettings _mailSettings;
    private readonly FirebasePushNotificationService _firebasePushNotificationService;
    private readonly InstagramServices _instagramServices;


    public EventsController(ILogger<EventsController> logger, EventsTypeService eventsType, EventsService eventService, IOptions<MailSettings> mailSettings, FirebasePushNotificationService firebasePushNotificationService, InstagramServices instagramServices)
    {
        _logger = logger;
        _eventsTypeService = eventsType;
        _eventService = eventService;
        _mailSettings = mailSettings.Value;
        _firebasePushNotificationService = firebasePushNotificationService;
        _instagramServices = instagramServices;
    }


    /// <summary>
    /// Retrieves a list of events based on optional filtering parameters.
    /// </summary>
    /// <param name="uid">Optional. Filters the list to include only events associated with the specified user ID.</param>
    /// <returns>Returns an ActionResult containing a list of events based on the applied filters.</returns>
    /// <response code="200">OK - The list of events was successfully retrieved.</response>
    /// <response code="400">Bad Request - Indicates an error or issue during the retrieval of events.</response>
    [HttpGet("", Name = "ListEvents")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Event>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    public async Task<ActionResult> ListEvents([FromQuery] string? uid = null)
    {
        try
        {
            // Call the method to retrieve the list of events based on the optional user ID filter
            return Ok(await _eventService.ListEvents(uid: uid));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "An error occurred during the retrieval of events.", ex);

            // If an exception occurs during retrieval, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "FatalError", "An error occurred during the retrieval of events.", ex.Message));
        }
    }


    /// <summary>
    /// Registers a new event.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST 
    ///     {
    ///         "name": "string",
    ///         "description": "string",
    ///         "eventDateTime": "2023-12-08T18:54:43.741Z",
    ///         "address": "string",
    ///         "latitude": 0,
    ///         "longitude": 0,
    ///         "eventTypes": [
    ///             "string"
    ///         ]
    ///     }
    ///     
    /// </remarks>
    /// <param name="request">The request payload containing information about the event.</param>
    /// <returns>Returns an ActionResult indicating the result of the event registration.</returns>
    /// <response code="200">OK - The event was successfully registered.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the registration.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPost("", Name = "EventRegister"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Register([FromBody] EventRequest request)
    {
        try
        {
            // Ensure that the user ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            // Retrieve the user ID from the claims in the HttpContext
            string userId = HttpContext.User.FindFirstValue("userID");

            // Validate each event type in the request
            foreach (string eventTypeId in request.EventTypes)
            {
                List<EventsType> types = await _eventsTypeService.ListEventsTypes(uid: eventTypeId);

                if (types.Count == 0)
                    return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Tipo de evento não encontrado", $"O tipo de evento especificado (ID: {eventTypeId}) não foi encontrado.", null));
            }

            // Create a new Event object and set its properties
            Event ev = new Event();
            ev.Name = request.Name;
            ev.Description = request.Description;
            ev.EventDateTime = request.EventDateTime;
            ev.CreatedBy = userId;
            ev.EventTypes = request.EventTypes;
            ev.Address = request.Address;
            ev.Latitude = request.Latitude;
            ev.Longitude = request.Longitude;
            ev.State = EventState.EmExecucao;

            // Call the method to register the event
            await _eventService.RegisterEvent(ev);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Evento registrado com sucesso", "O evento foi registrado com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Erro durante o registro do evento", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante o registro do evento.", ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST 
    ///     {
    ///         "name": "string",
    ///         "description": "string",
    ///         "eventDateTime": "2023-12-08T18:54:43.741Z",
    ///         "address": "string",
    ///         "latitude": 0,
    ///         "longitude": 0,
    ///         "State": "string",
    ///         "eventTypes": [
    ///             "string"
    ///         ]
    ///     }
    ///     
    /// </remarks>
    /// <param name="eventId">The ID of the event to be updated.</param>
    /// <param name="request">The request payload containing information about the event update.</param>
    /// <returns>Returns an ActionResult indicating the result of the event update.</returns>
    /// <response code="200">OK - The event was successfully updated.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the update.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPut("{eventId}", Name = "EventUpdate"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Update(string eventId, [FromBody] EventRequest request)
    {
        try
        {
            // Ensure that the requester's ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            string userId = HttpContext.User.FindFirstValue("userID");

            // Check if the requester is an administrator
            bool isAdmin = HttpContext.User.IsInRole(UserType.Admin.ToString());

            // Check if the event exists
            List<Event> existingEvent = await _eventService.ListEvents(uid: eventId);

            if (existingEvent == null || existingEvent.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Evento não encontrado", $"O evento com o ID {eventId} não foi encontrado.", null));

            // Check if the requester is the creator of the event or is an administrator
            if (existingEvent[0].CreatedBy != userId || !isAdmin)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Acesso não autorizado", "Você não tem permissão para atualizar este evento.", null));

            // Validate each event type in the request
            foreach (string eventTypeId in request.EventTypes)
            {
                List<EventsType> types = await _eventsTypeService.ListEventsTypes(uid: eventTypeId);

                if (types.Count == 0)
                    return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Tipo de evento não encontrado", $"O tipo de evento com o ID {eventTypeId} não foi encontrado.", null));
            }

            // Update the properties of the existing event
            existingEvent[0].Name = request.Name;
            existingEvent[0].Description = request.Description;
            existingEvent[0].EventDateTime = request.EventDateTime;
            existingEvent[0].Address = request.Address;
            existingEvent[0].Latitude = request.Latitude;
            existingEvent[0].Longitude = request.Longitude;
            existingEvent[0].EventTypes = request.EventTypes;
            existingEvent[0].State = request.State;

            // Call the method to update the event
            await _eventService.UpdateEvent(existingEvent[0]);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Evento atualizado com sucesso", "O evento foi atualizado com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Erro durante a atualização do evento", ex);

            // If an exception occurs during the update, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante a atualização do evento.", ex.Message));
        }
    }

    //Post /InviteParticipants/{eventID}
    /// <summary>
    /// Invites participants to an event.
    /// </summary>
    /// <remarks>
    ///     
    ///     /Post /InviteParticipants/{eventID}
    ///     [
    ///         {
    ///           "participantID": "string",
    ///           "participantTokens": [
    ///               "string"
    ///           ],
    ///           "participantEmail": "string",
    ///           "title": "string",
    ///           "body": "string"
    ///         }
    ///     ]
    /// 
    /// </remarks>
    /// <param name="eventID">The ID of the event to which participants are invited.</param>
    /// <param name="inviteParticipants">The list of participants to be invited.</param>
    /// <returns>Returns an ActionResult indicating the result of the participant invitation.</returns>
    /// <response code="200">OK - Participants were successfully invited to the event.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the participant invitation.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPost("InviteParticipants/{eventID}", Name = "InviteParticipants"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> InviteParticipants(string eventID, [FromBody] List<InviteParticipantRequest> inviteParticipants)
    {
        try
        {
            // Validate the list of participants
            if (inviteParticipants == null || inviteParticipants.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Lista de participantes inválida", "A lista de participantes está vazia ou é inválida.", null));

            foreach (var participant in inviteParticipants)
            {
                // Validate each participant
                if ((string.IsNullOrEmpty(participant.ParticipantEmail) || !Helper.ValidateEmail(participant.ParticipantEmail)) &&
                    (participant.ParticipantTokens == null || participant.ParticipantTokens.Count == 0 || string.IsNullOrEmpty(participant.ParticipantID)))
                {
                    return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Dados do participante inválidos", "Os dados do participante estão ausentes ou são inválidos.", null));
                }
            }

            foreach (var participant in inviteParticipants)
            {
                string codigo = null;
                ParticipantType type = ParticipantType.Convidado;

                if (!string.IsNullOrEmpty(participant.ParticipantEmail))
                {
                    Guid code = Guid.NewGuid(); // Generating a new GUID
                    type = ParticipantType.ConvidadoPorEmail;
                    codigo = code.ToString();
                    participant.ParticipantID = participant.ParticipantEmail;
                    Helper.SendEmail(_mailSettings, participant.ParticipantEmail, participant.Title, participant.Body);
                }
                else
                {
                    foreach (string token in participant.ParticipantTokens)
                    {
                        _firebasePushNotificationService.SendPushNotificationAsync(token, participant.Title, participant.Body);
                    }
                }

                Participant p = new Participant();
                p.Id = ObjectId.GenerateNewId().ToString();
                p.Codigo = codigo;
                p.Type = type;
                p.DtType = DateTime.Now;
                p.Id = participant.ParticipantID;

                // Call the method to save the participant for the specified event
                await _eventService.SaveParticipant(eventID, p);
            }

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Participantes convidados com sucesso", "Os participantes foram convidados para o evento com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante o convite para os participantes.", ex);

            // If an exception occurs during the invitation, return a BadRequest response with details.
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante o convite para os participantes.", ex.Message));
        }
    }

    //ParticipantStatus /Put
    /// <summary>
    /// Updates the status of participants for a specific event.
    /// </summary>
    /// <remarks>
    ///     
    ///     /Post /InviteParticipants/{eventID}
    ///     [
    ///         {
    ///           "participantID": "string",
    ///           "participantTokens": [
    ///               "string"
    ///           ],
    ///           "participantEmail": "string",
    ///           "title": "string",
    ///           "body": "string"
    ///         }
    ///     ]
    /// 
    /// </remarks>
    /// <param name="eventID">The ID of the event for which participant status is being updated.</param>
    /// <param name="participantStatus">The request payload containing participant status information.</param>
    /// <returns>Returns an ActionResult indicating the result of the participant status update.</returns>
    /// <response code="200">OK - Participant status was successfully updated.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the participant status update.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPut("ParticipantStatus/{eventID}", Name = "UpdateParticipantStatus"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ParticipantsStatus(string eventID, [FromBody] ParticipantStatusRequest participantStatus)
    {
        try
        {
            // Ensure that the requester's ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            string userId = HttpContext.User.FindFirstValue("userID");

            if (participantStatus.type == ParticipantType.ConvidadoPorEmail)
            {
                // Validate participant status request for ConvidadoPorEmail type
                if (string.IsNullOrEmpty(participantStatus.Codgio) || string.IsNullOrEmpty(participantStatus.ParticipantEmail) || !Helper.ValidateEmail(participantStatus.ParticipantEmail))
                    return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Solicitação de status de participante inválida", "A solicitação de status do participante está ausente ou é inválida.", null));

                // Retrieve participant based on email and code
                Participant p = await _eventService.GetParticipant(eventID, participantStatus.ParticipantEmail, participantStatus.Codgio);

                if (p == null)
                    return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Participante não encontrado", "O participante especificado não foi encontrado.", null));

                // Update participant status and details
                p.ParticipantID = userId;
                p.Status = participantStatus.Status;
                p.DtStatus = DateTime.Now;

                // Call the method to update the participant
                await _eventService.UpdateParticipant(eventID, participantStatus.ParticipantEmail, p);
            }
            else
            {
                // Retrieve participant based on user ID
                Participant p = await _eventService.GetParticipant(eventID, userId);

                if (p == null)
                {
                    // Create a new participant if not found
                    p = new Participant();
                    p.Id = ObjectId.GenerateNewId().ToString();
                    p.Status = participantStatus.Status;
                    p.DtStatus = DateTime.Now;
                    p.ParticipantID = userId;
                    p.Type = ParticipantType.ParticipanteRegular;
                    p.DtStatus = DateTime.Now;

                    // Call the method to save the participant
                    await _eventService.SaveParticipant(eventID, p);
                }
                else
                {
                    // Update participant status if already exists
                    p.Status = participantStatus.Status;
                    p.DtStatus = DateTime.Now;
                    await _eventService.UpdateParticipant(eventID, userId, p);
                }
            }

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Status do participante atualizado com sucesso", "O status do participante foi atualizado com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante a atualização do status do participante.", ex);

            // If an exception occurs during the update, return a BadRequest response with details.
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante a atualização do status do participante.", ex.Message));
        }
    }


    [HttpPost("post/image")]
    public async Task<IActionResult> PostImage([FromForm] InstagramPostImageRequest postModel, [FromForm] IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest(new { Message = "Nenhum arquivo de imagem fornecido" });
        }

        using (var stream = new MemoryStream())
        {
            //await postModel.ImageFile.CopyToAsync(stream);
            //var imagePath = "temp_image.jpg"; // Pode ajustar o nome e caminho conforme necessário
            //System.IO.File.WriteAllBytes(imagePath, stream.ToArray());

            bool isLogin = await _instagramServices.Login(postModel.Username, postModel.Password);

            if (isLogin)
            {
                if (await _instagramServices.PostImage(imageFile, postModel.Caption))
                {
                    return Ok(new { Message = "Imagem postada com sucesso" });
                }

                return BadRequest(new { Message = "Falha ao postar imagem" });
            }

            return BadRequest(new { Message = "Falha no login" });

            //if (await _instagramManager.PostImage(imagePath, postModel.Caption))
            //{
            //    return Ok(new { Message = "Imagem postada com sucesso" });
            //}

            //return BadRequest(new { Message = "Falha ao postar imagem" });
        }

        

         
    }



    //Get /States
    /// <summary>
    /// Retrieves a list of event states.
    /// </summary>
    /// <returns>An ActionResult containing a list of event states with their values and descriptions.</returns>
    /// <response code="200">OK - The list of event states was successfully retrieved.</response>
    /// <response code="400">Bad Request - Indicates an error or issue during the retrieval of event states.</response>
    [HttpGet("EventStates", Name = "EventStates")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    public ActionResult<List<object>> ListEventStates()
    {
        try
        {
            // Obtain all values from the EventState enum and create an anonymous type with properties Value and Description.
            var eventStates = Enum.GetValues(typeof(EventState))
                .Cast<EventState>()
                .Select(e => new
                {
                    Value = e.ToString(),
                    Description = Helper.GetDescription(e)
                })
                .ToList();  // Convert IEnumerable to List for better compatibility with ActionResult.

            // Return the list of event states as an OK response.
            return Ok(eventStates);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "An error occurred during the retrieval of event states.", ex);

            // If an exception occurs during retrieval, return a BadRequest response with details.
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "FatalError", "An error occurred during the retrieval of event states.", ex.Message));
        }
    }

    //Get /ParticipantStatus
    /// <summary>
    /// Retrieves a list of participant statuses.
    /// </summary>
    /// <returns>Returns an ActionResult containing a list of participant statuses with their values and descriptions.</returns>
    /// <response code="200">OK - The list of participant statuses was successfully retrieved.</response>
    /// <response code="400">Bad Request - Indicates an error or issue during the retrieval of participant statuses.</response>
    [HttpGet("ParticipantStatus", Name = "ParticipantStatus")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    public ActionResult<List<object>> ListParticipantStatus()
    {
        try
        {
            // Obtain all values from the ParticipantStatus enum and create an anonymous type with properties Value and Description.
            var participantStatuses = Enum.GetValues(typeof(ParticipantStatus))
                .Cast<ParticipantStatus>() // Corrected the enum type
                .Select(e => new
                {
                    Value = e.ToString(),
                    Description = Helper.GetDescription(e)
                })
                .ToList();  // Convert IEnumerable to List for better compatibility with ActionResult.

            // Return the list of participant statuses as an OK response.
            return Ok(participantStatuses);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "An error occurred during the retrieval of participant statuses.", ex);

            // If an exception occurs during retrieval, return a BadRequest response with details.
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "FatalError", "An error occurred during the retrieval of participant statuses.", ex.Message));
        }
    }

}
