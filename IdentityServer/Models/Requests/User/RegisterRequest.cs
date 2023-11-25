using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.Requests.User;

public class RegisterRequest
{
    [Required]
    public string username { get; set; }

    [Required]
    public string password { get; set; }

    [Required]
    public string email { get; set; }
}
