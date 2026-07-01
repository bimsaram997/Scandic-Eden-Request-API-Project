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
        private readonly IEmployeeService _employeeService;

        public RequestController(IRequestService requestService,
            IHubContext<NotificationHub> hubContext, IEmployeeService employeeService)
        {

            _requestService = requestService;
            _hubContext = hubContext;
            _employeeService = employeeService;
        }

        [HttpPost("placeBulkRequest")]
        public async Task<IActionResult> Create([FromBody] PlaceBulkRequestDto dto)
        {
            try
            {
                var result = await _requestService.PlaceRequestAsync(dto);
                var employee = await _employeeService.GetEmployeeById(result.EmployeeId);
                string senderEmail = employee?.Email ?? "Unknown Housekeeper";
                // Broadcast to the Team Leader monitoring channel
                await _hubContext.Clients.Group("ActiveLeadersDashboard")
                    .SendAsync("ReceiveNewRequestAlert", new
                    {
                        requestId = result.Id,
                        roomNumber = result.RoomNumber,
                        status = result.Status,
                        notes = result.Notes,
                        createdBy = senderEmail
                    });

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
                // 🟢 1. CRITICAL FIX: Fetch the request directly BEFORE updating it
                // (Make sure your RequestService has a method to get a request by ID)
                var originalRequest = await _requestService.GetRequestByIdAsync(id);
                int originalHousekeeperId = originalRequest?.EmployeeId ?? 0;

                // 2. Perform the actual status change
                var updated = await _requestService.ChangeStatusAsync(id, payload);

                // 🟢 3. CRITICAL FIX: Use the originalHousekeeperId instead of updated.EmployeeId
                if (updated != null && originalHousekeeperId > 0)
                {
                    // Fetch the original creator's data profile
                    var employee = await _employeeService.GetEmployeeById(originalHousekeeperId);

                    if (employee != null && !string.IsNullOrEmpty(employee.Email))
                    {
                        // Target the specific housekeeper group by cleaning up their email string
                        string cleanEmail = employee.Email.Replace("@", "_").Replace(".", "_");
                        string housekeeperChannel = $"User_{cleanEmail}";

                        await _hubContext.Clients.Group(housekeeperChannel)
                            .SendAsync("ReceiveStatusUpdate", new
                            {
                                requestId = updated.Id,
                                roomNumber = updated.RoomNumber,
                                status = updated.Status // 👈 Quick Tip: Make sure you pass updated.Status here instead of the whole object!
                            });
                    }
                }
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
