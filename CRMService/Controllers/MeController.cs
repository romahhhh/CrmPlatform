using CRMService.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers
{
    [ApiController]
    [Route("api/crm/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var userId = User.GetUserId();
            return Ok(new { userId });
        }
    }
}
