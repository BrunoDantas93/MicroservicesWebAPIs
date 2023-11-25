using IdentityServer.Models.MongoDB;
using MicroservicesHelpers;
using MicroservicesHelpers.Models;
using MicroservicesHelpers.Models.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer.Helpers.Authentication;

public class UserAuthHelper
{
    private readonly AuthenticationConfiguration _authConfig;
    private readonly AppSettings _appSettings;

    public UserAuthHelper(IOptions<AuthenticationConfiguration> options, IOptions<AppSettings> appSettings)
    {
        this._authConfig = options.Value;
        this._appSettings = appSettings.Value;
    }

    public TokenDetails GenerateToken(User user)
    {
        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_authConfig.AccessTokenExpirationMinutes);
        var refreshTokenExpires = DateTime.UtcNow.AddMinutes(_authConfig.RefreshTokenExpirationMinutes);

        var accessToken = GenerateJwtToken(user, accessTokenExpires, _authConfig.UserPrivateKey);
        var refreshToken = GenerateJwtToken(user, refreshTokenExpires, _authConfig.UserPrivateKey);

        return new TokenDetails
        {
            AccessToken = accessToken,
            AccessTokenExpiration = accessTokenExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpires
        };
    }

    private string GenerateJwtToken(User user, DateTime expiration, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new JwtSecurityToken
            (
                issuer: _authConfig.Issuer,
                audience: _authConfig.Audience,
                claims: new[] {
                    new Claim("user_type", "user"),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("userID", user.Id),
                    new Claim(ClaimTypes.Role, user.UserType.ToString())
                },
                expires: expiration,
                signingCredentials: new SigningCredentials(GetRsaSecurityKey(), SecurityAlgorithms.RsaSha256Signature)
            );

        var token = tokenHandler.WriteToken(tokenDescriptor);

        return token;
    }

    
    private RsaSecurityKey GetRsaSecurityKey()
    {
        // Load the RSA public key
        var rsa = RSA.Create();
            
        var publicKeyText = File.ReadAllText("private_key.pem");
        rsa.ImportFromPem(publicKeyText);

        return new RsaSecurityKey(rsa);
    }

    public bool ValidateUserRefToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.UserPrivateKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetRsaSecurityKey(),
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
