using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using LogProcessorAPI.Helpers;
using LogProcessorAPI.Models;
using LogProcessorAPI.Services;
using MicroservicesHelpers;
using MicroservicesHelpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Web;
using static MicroservicesHelpers.Enumerated;

namespace LogProcessorAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class LogProcessorController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly LogProcessor _logs;
    private readonly LogService _logService;

    public LogProcessorController(ILogger<LogProcessorController> logger, LogProcessor logs, LogService logService)
    {
        _logger = logger;
        _logs = logs;
        _logService = logService;
    }

    ///Get /
    /// <summary>
    /// Executes a command to update logs.
    /// </summary>
    /// <remarks>
    /// This endpoint is accessible only to users with the 'Admin' role.
    /// </remarks>
    /// <returns>Returns an ActionResult indicating the result of the command to update logs.</returns>
    /// <response code="200">OK - The command to update logs was successfully executed.</response>
    /// <response code="400">Bad Request - Indicates issues during the execution of the command.</response>
    /// <response code="401">Unauthorized - The user is not authorized to execute this command.</response>
    /// <response code="403">Forbidden - The user does not have the required role ('Admin') to execute this command.</response>
    [HttpGet("UpdateLogs", Name = "ComandToUpdateLogs"), Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ComandToUpdateLogs()
    {
        try
        {
            // Process logs
            await _logs.ProcessarLogs();

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Logs atualizados com sucesso.", "O comando para atualizar os Logs foi executado com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Ocorreu um erro durante o processamento de logs.", ex);

            // If an exception occurs during command execution, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro durante a execução do comando.", "Um erro ocorreu durante a execução do comando para atualizar os Logs.", ex.Message));
        }
    }

    [HttpGet("", Name = "ListLogs"), Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ListLogs([FromQuery] MicroservicesName? microservicesName = null, [FromQuery] string? dateFrom = null, [FromQuery] string? dateTo = null) 
    {
        try
        {
            DateTime? dFrom = null;
            DateTime? dTo = null;

            if (dateFrom != null && dateTo != null)
            {
                dFrom = DateTime.Parse(HttpUtility.UrlDecode(dateFrom));
                dTo = DateTime.Parse(HttpUtility.UrlDecode(dateTo));
            }

            // Process logs
            List<LogInformation> logs = await _logService.ListLogs(microservicesName, dFrom, dTo);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "", "", logs));
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Ocorreu um erro durante o processamento de logs.", ex);

            // If an exception occurs during command execution, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro durante a execução do comando.", "Um erro ocorreu durante a execução do comando para atualizar os Logs.", ex.Message));
        }
    }

    [HttpGet("ListMicroservicesName", Name = "ListMicroservicesName")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    public ActionResult<List<object>> ListMicroservicesName()
    {
        try
        {
            // Obtain all values from the ParticipantStatus enum and create an anonymous type with properties Value and Description.
            var microservicesName = Enum.GetValues(typeof(MicroservicesName))
                .Cast<MicroservicesName>() // Corrected the enum type
                .Select(e => new
                {
                    Value = e.ToString(),
                    Description = Helper.GetDescription(e)
                })
                .ToList();  // Convert IEnumerable to List for better compatibility with ActionResult.

            // Return the list of participant statuses as an OK response.
            return Ok(microservicesName);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "", ex);

            // If an exception occurs during retrieval, return a BadRequest response with details.
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "", "", ex.Message));
        }
    }


}
