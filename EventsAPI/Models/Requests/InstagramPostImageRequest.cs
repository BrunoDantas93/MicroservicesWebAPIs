using System.ComponentModel.DataAnnotations;

namespace EventsAPI.Models.Requests;

public class InstagramPostImageRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    //[Required]
    //public IFormFile ImageFile { get; set; }

    [Required]
    public string Caption { get; set; }
}
