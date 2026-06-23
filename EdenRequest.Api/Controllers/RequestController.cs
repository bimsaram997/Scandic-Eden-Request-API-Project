using EdenRequest.Api.DTO;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlaceBulkRequestDto dto)
        {
            try
            {
                var result = await _requestService.PlaceRequestAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("getAll")]
        public async Task<IActionResult> GetAllRequests([FromBody] RequestFilterDto filter)
        {
            // For now, we accept the empty filter and just fetch the active requests
            var requests = await _requestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) // Change 'int' to 'Guid' or 'string' if your IDs use a different type
        {
            var request = await _requestService.GetRequestByIdAsync(id);

            
            if (request == null)
            {
                return NotFound(new { message = $"Request with ID {id} not found." });
            }

            return Ok(request);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusPayload payload)
        {
            try
            {
                var updated = await _requestService.ChangeStatusAsync(id, payload.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public record UpdateStatusPayload(string Status);

    }
}
