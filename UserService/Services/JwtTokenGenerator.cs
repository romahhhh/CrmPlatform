using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

namespace UserService.Services
{
  public class JwtTokenGenerator : IJwtTokenGenerator
  {
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
      _options = options.Value;
    }

    public string Generate(User user)
    {
      var claims = new[]
      {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: _options.Issuer,
          audience: _options.Audience,
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
          signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
