using IdentityServer.Helpers.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserAuthHelper _authenticator;

    public UsersController(UserAuthHelper authenticator)
    {
        _authenticator = authenticator;
    }

    [HttpPost]
    public ActionResult Login()
    {
        try
        {
            return Ok(_authenticator.GenerateToken("Bruno"));
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPost("Hello")]
    public ActionResult<string> ert()
    {
        return "Hello";
    }

    [HttpGet]
    [Authorize]
    public ActionResult Logout()
    {
        return Ok("waddsadas");
    }

    [HttpGet("userendpoint")]
    [Authorize(Policy = "UserPolicy")]
    public IActionResult UserEndpoint()
    {
        // User-specific endpoint logic
        return Ok("This is a user endpoint.");
    }
}
