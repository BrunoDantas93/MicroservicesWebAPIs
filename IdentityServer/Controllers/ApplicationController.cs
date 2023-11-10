using IdentityServer.Helpers.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly AppAuthHelper _authenticator;

    public ApplicationController(AppAuthHelper authenticator)
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

    [HttpGet]
    [Authorize]
    public ActionResult Logout()
    {
        return Ok("waddsadas");
    }

    [HttpGet("applicationendpoint")]
    [Authorize(Policy = "ApplicationPolicy")]
    public IActionResult ApplicationEndpoint()
    {
        // Application-specific endpoint logic
        return Ok("This is an application endpoint.");
    }
}
