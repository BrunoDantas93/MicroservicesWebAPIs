using System.ComponentModel.DataAnnotations;
using static MicroservicesHelpers.Enumerated;

namespace CommunicationService.Models.Requests;

public class TranslateDeelp
{
    [Required]
    public string Message { get; set; }

    [Required]
    public LanguageCode Language { get; set; }
}
