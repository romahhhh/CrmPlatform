using CRMService.DTOs;
using CRMService.Extensions;
using CRMService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Controllers
{
    [ApiController]
    [Route("api/clients")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clients;

        public ClientsController(IClientService clients)
        {
            _clients = clients;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ClientResponse>>> GetAll()
        {
            var userId = User.GetUserId();
            var clients = await _clients.GetAllAsync(userId);
            return Ok(clients);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientResponse>> GetById(Guid id)
        {
            var userId = User.GetUserId();
            try
            {
                var client = await _clients.GetByIdAsync(userId, id);
                return Ok(client);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ClientResponse>> Create([FromBody] ClientCreateRequest request)
        {
            var userId = User.GetUserId();
            var client = await _clients.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClientResponse>> Update(Guid id, [FromBody] ClientUpdateRequest request)
        {
            var userId = User.GetUserId();
            try
            {
                var client = await _clients.UpdateAsync(userId, id, request);
                return Ok(client);
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
                await _clients.DeleteAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
