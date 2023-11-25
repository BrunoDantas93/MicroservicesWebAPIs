using IdentityServer.Helpers.Authentication;
using IdentityServer.Models.MongoDB;
using IdentityServer.Models.Requests.User;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using MicroservicesHelpers;
using MicroservicesHelpers.Models;
using static MicroservicesHelpers.Enumerated;
using MicroservicesHelpers.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Validation;
using IdentityServer4.Test;

namespace IdentityServer.Controllers;

/// <summary>
/// Controller responsible for handling user-related HTTP requests.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly UserAuthHelper _authenticator;
    private readonly UserService _usersService;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Constructor for UserController, initializing required services and dependencies.
    /// </summary>
    /// <param name="logger">Logger for logging messages.</param>
    /// <param name="authenticator">Helper class for user authentication.</param>
    /// <param name="usersService">Service for user-related operations.</param>
    /// <param name="appSettings">Application settings, including security-related settings.</param>
    public UserController(ILogger<UserController> logger, UserAuthHelper authenticator, UserService usersService, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _authenticator = authenticator;
        _usersService = usersService;
        _appSettings = appSettings.Value;
    }

    //Post /
    /// <summary>
    /// Handles the HTTP POST request for user registration.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     Post /
    ///     {
    ///         "username": "string",
    ///         "password": "string",
    ///         "email": "string@gmail.com"
    ///     }
    ///     
    /// </remarks>
    /// <param name="request">The registration request data received from the client.</param>
    /// <returns>Returns an ActionResult representing the result of the registration attempt.</returns>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if the requested username already exists
            bool usernameExists = await _usersService.GetUsername(request.username);

            // If the username already exists, return a BadRequest response
            if (usernameExists)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Erro ao registrar", "O nome de utilizador já existe.", null));

            // Create a new User object with the provided information
            User user = new User()
            {
                Username = request.username,
                Password = Helper.HashPassword(request.password, _appSettings.Salt),
                Email = request.email,
                RegisterDate = DateTime.UtcNow,
                UserType = UserType.User,
            };

            // Call the UsersService to create the new user
            await _usersService.CreateUsers(user);

            // Return an Ok response indicating successful user creation
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Sucesso", "Utilizador criado com sucesso.", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(400, "Ocorreu um erro durante o registro.", ex);

            // If an exception occurs during registration, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante o registro.", ex.Message));
        }
    }

    //Post /Login
    /// <summary>
    /// Handles the HTTP POST request for user login.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     Post /Login
    ///     {
    ///         "username": "string",
    ///         "password": "string"
    ///     }
    ///     
    /// </remarks>
    /// <param name="request">The login request data received from the client.</param>
    /// <returns>Returns an ActionResult representing the result of the login attempt.</returns>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    [HttpPost("Login", Name = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Attempt to retrieve user information based on the provided username and hashed password
            User user = await _usersService.GetLogin(request.username, Helper.HashPassword(request.password, _appSettings.Salt));

            // If no user is found, return a BadRequest response indicating invalid credentials
            if (user == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Credenciais inválidas", "Nome do utilizador ou password incorretos.", null));

            // Generate a token for the authenticated user
            TokenDetails token = _authenticator.GenerateToken(user);

            // Save the refresh token for the user
            await _usersService.SaveRefreshToken(user.Id, token.RefreshToken);

            // Return an Ok response with the generated token
            return Ok(token);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Ocorreu um erro durante o login.", ex);

            // If an exception occurs during login, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante o login.", ex.Message));
        }
    }

    //DELETE /Logout
    /// <summary>
    /// Encerra a sessão de um utilizador.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     DELETE /Logout
    ///     {
    ///     }
    ///     
    /// </remarks>
    /// <response code="200">Ok</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="400">Bad Request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpDelete("Logout", Name = "Logout"), Authorize]
    public async Task<ActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            // Check if the user is authenticated
            if (HttpContext.User.FindFirstValue("userID") == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Utilizador não autenticado", "O utilizador não está autenticado.", null));

            // Retrieve the user ID from the claims in the HttpContext
            string id = HttpContext.User.FindFirstValue("userID");

            await _usersService.DeleteRefreshToken(id, refreshToken);

            // Return an Ok response indicating successful logout
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Logout bem-sucedido", "A sessão do utilizador foi encerrada com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Erro durante o logout", ex);

            // If an exception occurs during logout, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante o logout.", ex.Message));
        }
    }


    //PUT /Refresh
    /// <summary>
    /// Create a new token for the user.
    /// </summary>
    /// <remarks>
    /// Example: 
    /// 
    ///     POST /Refresh
    ///     "token..."
    ///     
    /// </remarks>
    /// <param name="refreshRequest">refreshToken</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("Refresh", Name = "RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Refresh([FromBody] string refreshToken)
    {
        try
        {
            // Create a new instance of JwtSecurityTokenHandler to handle the refresh token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Read and parse the refresh token
            var jsonToken = tokenHandler.ReadToken(refreshToken) as JwtSecurityToken;

            // If the refresh token is invalid, return a BadRequest response
            if (jsonToken == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Token inválido", "O token de atualização fornecido é inválido.", null));

            // Retrieve the expiration time from the refresh token
            var exp = jsonToken.Payload.Exp;

            // Convert the expiration time to a DateTime object
            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp.Value).DateTime;

            // If the refresh token is expired, return a BadRequest response
            if (expDateTime < DateTime.UtcNow)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Token expirado", "O token de atualização fornecido está expirado.", null));

            // Retrieve the user ID claim from the refresh token
            var userIDClaim = jsonToken.Payload.Claims.FirstOrDefault(c => c.Type == "userID");

            // If the user ID claim is not found, return a BadRequest response
            if (userIDClaim == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Claim ausente", "O token de atualização não contém a reivindicação necessária.", null));

            // Retrieve the user information based on the user ID
            User user = await _usersService.GetUser(userIDClaim.Value);

            // If no user is found, return a BadRequest response indicating invalid user ID
            if (user == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Utilizador não encontrado", "O utilizador com o ID especificado não foi encontrado.", null));


            // Generate a new access token for the user
            TokenDetails token = _authenticator.GenerateToken(user);

            // Delete the used refresh token
            await _usersService.DeleteRefreshToken(userIDClaim.Value, refreshToken);

            // Return an Ok response with the new access token
            return Ok(token);
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Erro durante a atualização do token.", ex);

            // If an exception occurs during token refresh, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante a atualização do token.", ex.Message));
        }
    }


    // PUT /ChangePermissions/{id}
    /// <summary>
    /// Changes the permissions (UserType) of a user identified by the provided ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose permissions are to be changed.</param>
    /// <param name="type">The new user type to assign to the user (default is UserType.User).</param>
    /// <returns>Returns an ActionResult representing the result of the permissions change attempt.</returns>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [HttpPut("ChangePermissions/{id}", Name = "ChangePermissions"), Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePermissions(string id, [FromQuery] UserType type = UserType.User)
    {
        try
        {
            // Retrieve the user information based on the user ID
            User user = await _usersService.GetUser(id);

            // If no user is found, return a BadRequest response indicating invalid user ID
            if (user == null)
                return BadRequest(new MicroservicesResponse(MicroservicesCode.Validation, "Utilizador não encontrado", "O utilizador com o ID especificado não foi encontrado.", null));

            // Call the UsersService to change the user's permissions (UserType)
            await _usersService.ChangeUserType(id, type);

            // Return an Ok response indicating successful permissions change
            return Ok(new MicroservicesResponse(MicroservicesCode.OK, "Alteração de permissões bem-sucedida", "As permissões do utilizador foram alteradas com sucesso.", null));
        }
        catch (Exception ex)
        {
            // Log the exception for debugging and monitoring purposes
            _logger.LogError(400, "Erro durante a alteração de permissões", ex);

            // If an exception occurs during permissions change, return a BadRequest response with details
            return BadRequest(new MicroservicesResponse(MicroservicesCode.FatalError, "Erro fatal", "Ocorreu um erro durante a alteração de permissões.", ex.Message));
        }
    }

}
