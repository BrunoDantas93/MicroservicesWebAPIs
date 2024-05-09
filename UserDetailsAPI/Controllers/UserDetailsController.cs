using MicroservicesHelpers.Models;
using MicroservicesHelpers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using UserDetailsAPI.Models.MongoDB;
using UserDetailsAPI.Services;
using static MicroservicesHelpers.Enumerated;

namespace UserDetailsAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserDetailsController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly UsersDetailsService _usersDetailsService;
    private readonly ProfilePictureService _profilePictureService;

    public UserDetailsController(ILogger<UserDetailsController> logger, UsersDetailsService usersDetailsService, ProfilePictureService profilePictureService)
    {
        _logger = logger;
        _usersDetailsService = usersDetailsService;
        _profilePictureService = profilePictureService;
    }

    // Get /ListUsersDetails
    /// <summary>
    /// Retrieves details of users.
    /// </summary>
    /// <param name="uID">Optional. The ID of the user for whom details are requested.</param>
    /// <returns>Returns an ActionResult containing the details of users.</returns>
    /// <response code="200">OK - User details were successfully retrieved.</response>
    [HttpGet("", Name = "ListUsersDetails")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ListUsersDetails([FromQuery] string? uID = null)
    {
        try
        {
            // Call the method to retrieve user details
            List<UserDetails> details = await _usersDetailsService.ListUsersDetails(uID);

            // Return a successful response
            return Ok(details);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Erro durante a listagem de detalhes de usuários.", ex);

            // If an exception occurs during retrieval, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante a listagem de detalhes de usuários.", ex.Message));
        }
    }

    // Post /RegisterUserDetails
    /// <summary>
    /// Registers details for a user.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST 
    ///     {
    ///         "id": "string",
    ///         "firstName": "string",
    ///         "lastName": "string",
    ///         "address": "string",
    ///         "nationality": "string",
    ///         "birthDate": "2023-12-13",
    ///         "gender": "string"
    ///     }
    ///     
    /// </remarks>
    /// <param name="detailsRequest">The request payload containing user details to be registered.</param>
    /// <returns>Returns an ActionResult indicating the result of the user details registration.</returns>
    /// <response code="200">OK - User details were successfully registered.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the user details registration.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPost("", Name = "RegisterUserDetails"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> RegisterDetails([FromBody] UserDetails detailsRequest)
    {
        try
        {
            // Ensure that the requester's ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            // Retrieve the user ID from the claims in the HttpContext
            string userId = HttpContext.User.FindFirstValue("userID");

            // Check if user details already exist
            List<UserDetails> ud = await _usersDetailsService.ListUsersDetails(userId);

            if (ud.Count > 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Detalhes do utilizador já existem", "Os detalhes do utilizador especificado já existem.", null));

            // Create a UserDetails object with the provided details
            UserDetails details = new UserDetails();
            details.Id = userId;
            details.FirstName = detailsRequest.FirstName;
            details.LastName = detailsRequest.LastName;
            details.Address = detailsRequest.Address;
            details.Gender = detailsRequest.Gender;
            details.BirthDate = detailsRequest.BirthDate;
            details.Nationality = detailsRequest.Nationality;

            // Call the method to register user details
            await _usersDetailsService.RegisterUserDetails(details);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Detalhes do utilizador registrados com sucesso", "Os detalhes do utilizador foram registrados com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante o registro dos detalhes do utilizador.", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante o registro dos detalhes do utilizador.", ex.Message));
        }
    }

    // Put /UpdateUserDetails
    /// <summary>
    /// Updates details for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST 
    ///     {
    ///         "id": "string",
    ///         "firstName": "string",
    ///         "lastName": "string",
    ///         "address": "string",
    ///         "nationality": "string",
    ///         "birthDate": "2023-12-13",
    ///         "gender": "string"
    ///     }
    ///     
    /// </remarks>
    /// <param name="detailsRequest">The request payload containing updated user details.</param>
    /// <returns>Returns an ActionResult indicating the result of the user details update.</returns>
    /// <response code="200">OK - User details were successfully updated.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the user details update.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPut("", Name = "UpdateUserDetails"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateDetails([FromBody] UserDetails detailsRequest)
    {
        try
        {
            // Ensure that the requester's ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            // Retrieve the user ID from the claims in the HttpContext
            string userId = HttpContext.User.FindFirstValue("userID");

            // Check if user details exist
            List<UserDetails> ud = await _usersDetailsService.ListUsersDetails(userId);

            if (ud.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Detalhes do usuário não encontrados", "Detalhes do usuário para o usuário especificado não foram encontrados.", null));

            // Update user details with the provided values
            ud[0].FirstName = detailsRequest.FirstName;
            ud[0].LastName = detailsRequest.LastName;
            ud[0].Address = detailsRequest.Address;
            ud[0].Gender = detailsRequest.Gender;
            ud[0].BirthDate = detailsRequest.BirthDate;
            ud[0].Nationality = detailsRequest.Nationality;

            // Call the method to update user details
            await _usersDetailsService.UpdateUsersDetails(ud[0]);

            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Detalhes do usuário atualizados com sucesso", "Os detalhes do usuário foram atualizados com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante a atualização dos detalhes do usuário.", ex);

            // If an exception occurs during the update, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante a atualização dos detalhes do usuário.", ex.Message));
        }
    }


    // Post /ProfilePicture
    /// <summary>
    /// Sets or updates the profile picture for the authenticated user.
    /// </summary>
    /// <remarks>
    /// <param name="profilePicture">The form file containing the profile picture to be set or updated.</param>
    /// <returns>Returns an ActionResult indicating the result of setting or updating the profile picture.</returns>
    /// <response code="200">OK - Profile picture was successfully set or updated.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the profile picture setting or updating.</response>
    /// <response code="401">Unauthorized - The user is not authorized to perform this action.</response>
    [HttpPost("ProfilePicture", Name = "SetProfilePicture"), Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SetProfilePicture([FromForm] IFormFile profilePicture)
    {
        try
        {
            // Ensure that the requester's ID claim exists
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "ID do solicitante ausente", "Não foi possível obter informações do solicitante autenticado.", null));

            // Retrieve the user ID from the claims in the HttpContext
            string userId = HttpContext.User.FindFirstValue("userID");

            // Obtain user details
            List<UserDetails> userDetails = await _usersDetailsService.ListUsersDetails(userId);

            if (userDetails.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Detalhes do usuário não encontrados", "Detalhes do usuário para o usuário especificado não foram encontrados.", null));

            // Obtain user's existing profile pictures
            List<ProfilePictures> existingPictures = await _profilePictureService.ListProfilePictures(userId);

            ProfilePictures profilePic = new ProfilePictures();

            // Convert the image from the form into bytes
            using (var memoryStream = new MemoryStream())
            {
                await profilePicture.CopyToAsync(memoryStream);
                profilePic.ProfilePicture = memoryStream.ToArray();
            }

            // Check if a profile picture already exists
            if (existingPictures.Count == 0)
            {
                profilePic.Id = userId;

                await _profilePictureService.RegisterProfilePicture(profilePic);

                // Return a successful response
                return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Foto de perfil registrada com sucesso", "A foto de perfil foi registrada com sucesso.", null));
            }
            else
            {
                profilePic.Id = existingPictures[0].Id;

                await _profilePictureService.UpdateProfilePicture(profilePic);

                // Return a successful response
                return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Foto de perfil atualizada com sucesso", "A foto de perfil foi atualizada com sucesso.", null));
            }
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante o processo da foto de perfil.", ex);

            // If an exception occurs during the process, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante o processo da foto de perfil.", ex.Message));
        }
    }


    //Get /ProfilePicture    
    /// <summary>
    /// Retrieves the profile picture of the specified user or the authenticated user if no user ID is provided.
    /// </summary>
    /// <param name="uID">Optional. The user ID for which to retrieve the profile picture. If not provided, retrieves the profile picture of the authenticated user.</param>
    /// <returns>Returns an ActionResult containing the profile picture of the specified user or the authenticated user.</returns>
    /// <response code="200">OK - Profile picture successfully retrieved.</response>
    /// <response code="400">Bad Request - Indicates validation errors or issues during the profile picture retrieval.</response>
    [HttpGet("ProfilePicture", Name = "ProfilePicture")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MicroservicesResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MicroservicesResponse))]
    public async Task<ActionResult> ProfilePicture([FromQuery] string? uID = null)
    {
        try
        {
            List<UserDetails> userDetails = await _usersDetailsService.ListUsersDetails(uID);

            if (userDetails.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Detalhes do usuário não encontrados", "Os detalhes do usuário especificado não foram encontrados.", null));


            List<ProfilePictures> profilePictures = await _profilePictureService.ListProfilePictures(uID);

            if (profilePictures.Count == 0)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Foto de perfil não encontrada", "A foto de perfil para o usuário especificado não foi encontrada.", null));


            // Return a successful response
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Foto de perfil recuperada com sucesso", "", profilePictures[0]));
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(400, "Ocorreu um erro durante a recuperação da foto de perfil.", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro Fatal", "Ocorreu um erro durante a recuperação da foto de perfil.", ex.Message));
        }
    }

}
