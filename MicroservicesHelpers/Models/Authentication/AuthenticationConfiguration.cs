namespace MicroservicesHelpers.Models.Authentication;

public class AuthenticationConfiguration
{
    public string UserAccessTokenSecret { get; set; }
    public string UserRefreshTokenSecret { get; set; }
    public string AppAccessTokenSecret { get; set; }
    public string AppRefreshTokenSecret { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
