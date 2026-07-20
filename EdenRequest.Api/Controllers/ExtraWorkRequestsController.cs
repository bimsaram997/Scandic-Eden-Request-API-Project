using EdenRequest.Api.DTO;
using EdenRequest.Api.Dtos;
using EdenRequest.Api.Services;
using EdenRequest.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using EdenRequest.Api.Requests;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraWorkRequestsController : ControllerBase
    {
        private readonly IExtraWorkRequestService _extraWorkRequestService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmployeeService _employeeService;
        private readonly NotificationService _notificationService;

        // Inject the SignalR Hub context and employee/notification infrastructure services
        public ExtraWorkRequestsController(
            IExtraWorkRequestService extraWorkRequestService,
            IHubContext<NotificationHub> hubContext,
            IEmployeeService employeeService,
            NotificationService notificationService)
        {
            _extraWorkRequestService = extraWorkRequestService;
            _hubContext = hubContext;
            _employeeService = employeeService;
            _notificationService = notificationService;
        }

        // GET: api/ExtraWorkRequests/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraWorkRequestDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExtraWorkRequestDto>> GetById(int id)
        {
            var result = await _extraWorkRequestService.GetRequestByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Extra work request with ID {id} was not found." });
            }

            return Ok(result);
        }

        // POST: api/ExtraWorkRequests/createExtraWorkRequest
        [HttpPost("createExtraWorkRequest")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ExtraWorkRequestDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExtraWorkRequestDto>> Create([FromBody] CreateExtraWorkRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Persist the new extra work request inside the database
                var createdDto = await _extraWorkRequestService.CreateRequestAsync(dto);

                // 2. Safely process real-time and push alerts to the assigned Housekeeper
                // Ensure there is an explicit housekeeper assigned (AssignedToId is greater than 0)
                if (createdDto != null && createdDto.AssignedToId > 0)
                {
                    try
                    {
                        var employee = await _employeeService.GetEmployeeById(createdDto.AssignedToId);

                        if (employee != null && !string.IsNullOrEmpty(employee.Email))
                        {
                            // 🟢 A. Route Live SignalR Update Group Event
                            string cleanEmail = employee.Email.Replace("@", "_").Replace(".", "_");
                            string housekeeperChannel = $"User_{cleanEmail}";

                            await _hubContext.Clients.Group(housekeeperChannel)
                                .SendAsync("ReceiveNewExtraWorkAlert", new
                                {
                                    requestId = createdDto.Id,
                                    roomNumber = createdDto.RoomNumber,
                                    listNumber = createdDto.ListNumber,
                                    notes = createdDto.Notes
                                });

                            // 🟢 B. Dispatch Web Push Notification (If user has valid active device profile endpoints)
                            if (!string.IsNullOrEmpty(employee.PushEndpoint))
                            {
                                string pushTitle = "🧹 New Extra Work Assigned!";
                                string pushBody = $"Room {createdDto.RoomNumber} (List {createdDto.ListNumber}) has been added to your tasks.";
                                string targetUrl = $"/workspace/extra-work-request-detail/{createdDto.Id}";

                                await _notificationService.SendNotificationToEmployeeAsync(
                                    employee.Id,
                                    pushTitle,
                                    pushBody,
                                    targetUrl
                                );
                            }
                        }
                    }
                    catch (Exception alertEx)
                    {
                        // Wrap inside a nested catch so a notification transmission failure 
                        // never blocks the 201 Created database state return payload
                        Console.WriteLine($"Background alert dispatching encountered an error: {alertEx.Message}");
                    }
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdDto.Id },
                    createdDto
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraWorkRequestDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateExtraWorkRequestDto payload)
        {
            try
            {
                var originalRequest = await _extraWorkRequestService.GetRequestByIdAsync(id);
                if (originalRequest == null)
                {
                    return NotFound(new { message = $"Extra work request with ID {id} was not found." });
                }

                var updated = await _extraWorkRequestService.UpdateStatusAsync(id, payload);
                if (updated == null)
                {
                    return BadRequest(new { message = "Failed to update the status." });
                }

                var employee = await _employeeService.GetEmployeeById(updated.AssignedToId);
                string senderEmail = employee?.Email ?? "Unknown Housekeeper";

                // 🟢 A. Live SignalR Broadcast
                try
                {
                    await _hubContext.Clients.Group("ActiveLeadersDashboard")
                        .SendAsync("ReceiveExtraWorkStatusUpdateForLeader", new
                        {
                            requestId = updated.Id,
                            roomNumber = updated.RoomNumber,
                            status = updated.Status,
                            notes = updated.Notes,
                            createdBy = senderEmail
                        });
                }
                catch (Exception sigEx)
                {
                    Console.WriteLine($"SignalR live update failed: {sigEx.Message}");
                }

                // 🟢 B. Target Push Notifications
                try
                {
                    var targetLeaders = await _employeeService.GetEmployeesByRoleAsync("TeamLeader");

                    if (targetLeaders != null)
                    {
                        foreach (var baseLeader in targetLeaders)
                        {
                            try
                            {
                                // 🔥 CRITICAL FIX: Explicitly fetch a FRESH instance of this leader 
                                // from the DB by their ID to ensure the newly synced PushEndpoint is loaded!
                                var leader = await _employeeService.GetEmployeeById(baseLeader.Id);

                                // Fallback check to look at both lowercase/uppercase variations if your DTO maps fields dynamically
                                if (leader != null && (!string.IsNullOrEmpty(leader.PushEndpoint)))
                                {
                                    string pushTitle = "🚨 Extra Work Request is Updated!";
                                    string pushBody = $"Room {updated.RoomNumber} status changed to '{updated.Status}' by {senderEmail}.";
                                    string targetUrl = $"/workspace/extra-work-request-detail/{updated.Id}";

                                    await _notificationService.SendNotificationToEmployeeAsync(
                                        leader.Id,
                                        pushTitle,
                                        pushBody,
                                        targetUrl
                                    );
                                }
                            }
                            catch (Exception singleLeaderEx)
                            {
                                Console.WriteLine($"[Push Error] Failed sending to Leader: {singleLeaderEx.Message}");
                            }
                        }
                    }
                }
                catch (Exception pushLoopEx)
                {
                    Console.WriteLine($"Background alert dispatching encountered an error: {pushLoopEx.Message}");
                }

                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<ExtraWorkRequestDto>))]
        public async Task<ActionResult<PagedResponse<ExtraWorkRequestDto>>> GetAll([FromBody] AllExtraWorkQueryDto query)
        {
            if (query == null)
            {
                query = new AllExtraWorkQueryDto { Page = 1, PageSize = 6, IsToday = true };
            }

            try
            {
                var response = await _extraWorkRequestService.GetAllExtraRequestsAsync(query);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error getting filtered extra requests: {ex.Message}" });
            }
        }
    }
}