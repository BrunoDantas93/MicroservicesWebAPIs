using MicroservicesHelpers.Models;
using MicroservicesHelpers.Models.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServer.Helpers.Authentication;

public class AppAuthHelper
{
    private readonly AuthenticationConfiguration _authConfig;
    private readonly AppSettings _appSettings;

    public AppAuthHelper(IOptions<AuthenticationConfiguration> options, IOptions<AppSettings> op)
    {
        this._authConfig = options.Value;
        this._appSettings = op.Value;
    }

    public TokenDetails GenerateToken(string applicationId)
    {
        var expires = DateTime.UtcNow.AddMinutes(_authConfig.AccessTokenExpirationMinutes);
        var token = GenerateJwtApiKey(applicationId, expires, _authConfig.AppPrivateKey);

        return new TokenDetails
        {
            AccessToken = token,
            AccessTokenExpiration = expires
        };
    }
    private string GenerateJwtApiKey(string applicationId, DateTime expiration, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("application_type", "api_key"),
                new Claim(ClaimTypes.Name, applicationId)
            }),

            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateJwtToken(string applicationId, DateTime expiration, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new JwtSecurityToken
        (
            issuer: _authConfig.Issuer,
            audience: _authConfig.Audience,
            claims: new[] {
                new Claim("application_type", "application"),
                new Claim(ClaimTypes.Name, applicationId)
            },
            expires: expiration,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        ); ;

        return tokenHandler.WriteToken(tokenDescriptor);
    }

    public bool ValidateAppToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.AppPrivateKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero

            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
