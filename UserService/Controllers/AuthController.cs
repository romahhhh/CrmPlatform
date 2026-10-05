using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using UserService.Dtos;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _auth.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _auth.LoginAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
        
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteMe()
        {
            try
            {
                var sub = User.FindFirst("sub")?.Value 
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
                    throw new InvalidOperationException("Не удалось определить пользователя из токена.");

                await _auth.DeleteAsync(userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<ActionResult<AuthResponse>> UpdateMe([FromBody] UpdateUserRequest request)
        {
            try
            {
                var sub = User.FindFirst("sub")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
                    throw new InvalidOperationException("Не удалось определить пользователя из токена.");

                var response = await _auth.UpdateAsync(userId, request);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            return Ok(new { userId, email });
        }
    }
}
