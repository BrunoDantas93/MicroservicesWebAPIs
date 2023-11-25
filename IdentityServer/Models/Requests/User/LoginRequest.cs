using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.Requests.User;

public class LoginRequest
{
    [Required]
    public string username { get; set; }

    [Required]
    public string password { get; set; }
}
