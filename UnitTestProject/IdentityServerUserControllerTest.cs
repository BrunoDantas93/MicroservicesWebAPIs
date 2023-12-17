using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityServer.Helpers.Authentication;
using MicroservicesHelpers.Models;
using MicroservicesHelpers.Models.Authentication;
using IdentityServer.Models.MongoDB;
using IdentityServer.Models.Requests.User;
using IdentityServer.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer.Models;
using Microsoft.AspNetCore.Http;

namespace UnitTestProject;

[TestClass]
public class IdentityServerUserControllerTest
{
    private readonly UserController _userController;
    private readonly Mock<UserService> _userServiceMock;
    private readonly Mock<ILogger<UserController>> _loggerMock;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IOptions<AuthenticationConfiguration> _authConfig;
    private readonly IOptions<ConnectionConfigurations> _connectionConfigurations;
    private readonly IOptions<MailSettings> _mailSettings;
    private readonly UserAuthHelper _userAuthHelper; // Use the actual UserAuthHelper instead of a mock

    public static TokenDetails token = new TokenDetails();


    public IdentityServerUserControllerTest()
    {
        _loggerMock = new Mock<ILogger<UserController>>();

        // Read configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
        services.Configure<ConnectionConfigurations>(configuration.GetSection("ConnectionStrings"));
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

        var serviceProvider = services.BuildServiceProvider();

        _appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();
        _authConfig = serviceProvider.GetRequiredService<IOptions<AuthenticationConfiguration>>();
        _connectionConfigurations = serviceProvider.GetRequiredService<IOptions<ConnectionConfigurations>>();
        _mailSettings = serviceProvider.GetRequiredService<IOptions<MailSettings>>();

        _connectionConfigurations.Value.DatabaseName = "MicroservicesWebAPIsTest";

        _userServiceMock = new Mock<UserService>(
            Mock.Of<ILogger<UserController>>(),
            Options.Create(_connectionConfigurations.Value) // Use Options.Create to create IOptions<ConnectionConfigurations>
        );

        _userAuthHelper = new UserAuthHelper(_authConfig, _appSettings);

        _userController = new UserController(_loggerMock.Object, _userAuthHelper, _userServiceMock.Object, _appSettings, _mailSettings);
    }

    /// <summary>
    /// Validates the user registration process with a valid registration request.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task ValidRegistrationTest()
    {
        Random rnd = new Random();
        int num = rnd.Next();

        // Arrange
        var validRegisterRequest = new RegisterRequest
        {
            username = $"ValidUsername{num}",
            password = "StrongPassword123!",
            email = $"validemail{num}@gmail.com"
        };

        // Act
        var result = await _userController.Register(validRegisterRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult)); // Ensure the result is of type OkObjectResult
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode); // Check if the status code is 200 OK
        Assert.IsNotNull(okResult.Value); // Ensure the response has a value
    }

    /// <summary>
    /// Tests the user registration process with an invalid email format.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task InvalidEmailFormatTest()
    {
        // Arrange
        var invalidEmailRequest = new RegisterRequest
        {
            username = "InvalidEmailUser",
            password = "StrongPassword123!",
            email = "invalidemail" // Invalid email format
        };

        // Act
        var result = await _userController.Register(invalidEmailRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult)); // Ensure the result is of type BadRequestObjectResult
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode); // Check if the status code is 400 Bad Request
        Assert.IsNotNull(badRequestResult.Value); // Ensure the response has a value
    }

    /// <summary>
    /// Tests the user registration process with a weak password.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task WeakPasswordTest()
    {
        // Arrange
        var weakPasswordRequest = new RegisterRequest
        {
            username = "WeakPasswordUser",
            password = "weak", // Weak password
            email = "weakpassword@gmail.com"
        };

        // Act
        var result = await _userController.Register(weakPasswordRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.IsNotNull(badRequestResult.Value);
    }

    /// <summary>
    /// Tests the user registration process with an existing username.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExistingUsernameTest()
    {
        // Arrange
        var existingUsernameRequest = new RegisterRequest
        {
            username = "ValidUsername",
            password = "StrongPassword123!",
            email = "existinguser@gmail.com"
        };
               
        // Act
        var result = await _userController.Register(existingUsernameRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.IsNotNull(badRequestResult.Value);
    }

    /// <summary>
    /// Tests the user login process with valid login credentials.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task ValidLoginTest()
    {
        // Arrange
        var validLoginRequest = new LoginRequest
        {
            username = "ValidUsername",
            password = "StrongPassword123!"
        };

        // Act
        var result = await _userController.Login(validLoginRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.IsNotNull(okResult.Value);

        token = (TokenDetails)okResult.Value;
    }

    /// <summary>
    /// Tests the user login process with invalid login credentials.
    /// </summary>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task InvalidCredentialsTest()
    {
        // Arrange
        var invalidLoginRequest = new LoginRequest
        {
            username = "InvalidUsername",
            password = "InvalidPassword"
        };

        // Act
        var result = await _userController.Login(invalidLoginRequest);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.IsNotNull(badRequestResult.Value);
    }
}