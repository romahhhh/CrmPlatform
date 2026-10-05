using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CRMService.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Не удалось определить пользователя из токена.");

            return userId;
        }
    }
}
