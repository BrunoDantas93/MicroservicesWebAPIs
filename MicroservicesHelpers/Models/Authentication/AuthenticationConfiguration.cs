namespace MicroservicesHelpers.Models.Authentication;

public class AuthenticationConfiguration
{
    public string UserPrivateKey { get; set; }
    public string UserPublicKey { get; set; }
    public string AppPrivateKey { get; set; }
    public string AppPublicKey { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
