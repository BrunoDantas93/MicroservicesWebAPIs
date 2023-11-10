using MicroservicesHelpers.Models.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServer.Helpers.Authentication;

public class UserAuthHelper
{
    private readonly AuthenticationConfiguration _authConfig;

    public UserAuthHelper(IOptions<AuthenticationConfiguration> options)
    {
        this._authConfig = options.Value;
    }

    public TokenDetails GenerateToken(string userId)
    {
        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_authConfig.AccessTokenExpirationMinutes);
        var refreshTokenExpires = DateTime.UtcNow.AddMinutes(_authConfig.RefreshTokenExpirationMinutes);

        var accessToken = GenerateJwtToken(userId, accessTokenExpires, _authConfig.UserAccessTokenSecret);
        var refreshToken = GenerateJwtToken(userId, refreshTokenExpires, _authConfig.UserRefreshTokenSecret);

        return new TokenDetails
        {
            AccessToken = accessToken,
            AccessTokenExpiration = accessTokenExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpires
        };
    }

    private string GenerateJwtToken(string userId, DateTime expiration, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new JwtSecurityToken
            (
                issuer: _authConfig.Issuer,
                audience: _authConfig.Audience,
                claims: new[] { 
                    new Claim("user_type", "user"),
                    new Claim(ClaimTypes.Name, userId) 
                },
                expires: expiration,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );

        var token = tokenHandler.WriteToken(tokenDescriptor);

        return token;
    }

    public bool ValidateUserRefToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.UserRefreshTokenSecret);

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
