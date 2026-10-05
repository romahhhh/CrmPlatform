using CRMService.DTOs;
using CRMService.Extensions;
using CRMService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessions;

        public SessionsController(ISessionService sessions)
        {
            _sessions = sessions;
        }

        [HttpGet("by-client/{clientId:guid}")]
        public async Task<ActionResult<IReadOnlyList<SessionResponse>>> GetByClient(Guid clientId)
        {
            var userId = User.GetUserId();
            try
            {
                var sessions = await _sessions.GetByClientAsync(userId, clientId);
                return Ok(sessions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SessionResponse>> GetById(Guid id)
        {
            var userId = User.GetUserId();
            try
            {
                var session = await _sessions.GetByIdAsync(userId, id);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SessionResponse>> Create([FromBody] SessionCreateRequest request)
        {
            var userId = User.GetUserId();
            try
            {
                var session = await _sessions.CreateAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SessionResponse>> Update(Guid id, [FromBody] SessionUpdateRequest request)
        {
            var userId = User.GetUserId();
            try
            {
                var session = await _sessions.UpdateAsync(userId, id, request);
                return Ok(session);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.GetUserId();
            try
            {
                await _sessions.DeleteAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
