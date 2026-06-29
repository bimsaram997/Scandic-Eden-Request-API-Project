using EdenRequest.Api.DTO;
using EdenRequest.Api.Hubs;
using EdenRequest.Api.Requests;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public RequestController(IRequestService requestService, IHubContext<NotificationHub> hubContext)
        {
            _requestService = requestService;
            _hubContext = hubContext;
        }

        [HttpPost("placeBulkRequest")]
        public async Task<IActionResult> Create([FromBody] PlaceBulkRequestDto dto)
        {
            try
            {
                var result = await _requestService.PlaceRequestAsync(dto);
                await _hubContext.Clients.Group("ActiveLeadersDashboard")
                    .SendAsync("NewRequestReceived", result);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRquestHeaderRequest payload)
        {
            try
            {
                var updated = await _requestService.ChangeStatusAsync(id, payload);
                await _hubContext.Clients.Group($"Employee_{updated.EmployeeId}")
                    .SendAsync("RequestStatusUpdated", updated);
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

        [HttpPost("employee/{employeeId}/history")]
        public async Task<IActionResult> GetHistory(int employeeId, [FromQuery] bool isTeamLeader, [FromBody] HistoryQueryDto query)
        {
            // 1. Fallback protection if the body mapping object initializes as null
            if (query == null)
            {
                query = new HistoryQueryDto { Page = 1, PageSize = 6 };
            }

            try
            {
                //  Pass the logged-in user context, the role flag, and the ENTIRE filter object down!
                var response = await _requestService.GetEmployeeHistoryAsync(employeeId, isTeamLeader, query);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error routing filtered history records: {ex.Message}");
            }
        }
        public record UpdateStatusPayload(string Status, int UpdatedBy);

    }
}
