using EdenRequest.Api.DTO;
using EdenRequest.Api.Hubs;
using EdenRequest.Api.Requests;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebPush;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmployeeService _employeeService;
        private readonly NotificationService _notificationService;

        public RequestController(IRequestService requestService,
            IHubContext<NotificationHub> hubContext, IEmployeeService employeeService, NotificationService notificationService)
        {

            _requestService = requestService;
            _hubContext = hubContext;
            _employeeService = employeeService;
            _notificationService = notificationService;
        }

        [HttpPost("placeBulkRequest")]
        public async Task<IActionResult> Create([FromBody] PlaceBulkRequestDto dto)
        {
            try
            {
                var result = await _requestService.PlaceRequestAsync(dto);
                var employee = await _employeeService.GetEmployeeById(result.EmployeeId);
                string senderEmail = employee?.Email ?? "Unknown Housekeeper";
          
                await _hubContext.Clients.Group("ActiveLeadersDashboard")
                    .SendAsync("ReceiveNewRequestAlert", new
                    {
                        requestId = result.Id,
                        roomNumber = result.RoomNumber,
                        status = result.Status,
                        notes = result.Notes,
                        createdBy = senderEmail
                    });
                try
                {
                   
                    var targetLeaders = await _employeeService.GetEmployeesByRoleAsync("TeamLeader");

                    foreach (var leader in targetLeaders)
                    {
                        if (string.IsNullOrEmpty(leader.Email)) continue;
                        bool isCurrentlyOnline = EdenRequest.Api.Hubs.NotificationHub.ActiveUsers.ContainsKey(leader.Email.ToLower().Trim());

                        if (isCurrentlyOnline)
                        {
                            string pushTitle = "🚨 New Bulk Request!";
                            string pushBody = $"Room {result.RoomNumber} submitted by {senderEmail}.";
                            string targetUrl =  $"/workspace/requests-component/{result.Id}";

                            await _notificationService.SendNotificationToEmployeeAsync(
                                leader.Id,
                                pushTitle,
                                pushBody,
                                targetUrl
                            );
                        }
                    }
                }
                catch (Exception pushEx)
                {
                    return NotFound(pushEx.Message);
                }

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
            var requests = await _requestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) 
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
                var originalRequest = await _requestService.GetRequestByIdAsync(id);
                int originalHousekeeperId = originalRequest?.EmployeeId ?? 0;

                var updated = await _requestService.ChangeStatusAsync(id, payload);
                if (updated != null && originalHousekeeperId > 0)
                {
                    var employee = await _employeeService.GetEmployeeById(originalHousekeeperId);

                    if (employee != null && !string.IsNullOrEmpty(employee.Email))
                    {
                        string cleanEmail = employee.Email.Replace("@", "_").Replace(".", "_");
                        string housekeeperChannel = $"User_{cleanEmail}";

                        await _hubContext.Clients.Group(housekeeperChannel)
                            .SendAsync("ReceiveStatusUpdate", new
                            {
                                requestId = updated.Id,
                                roomNumber = updated.RoomNumber,
                                status = updated.Status
                            });
                    }

                    string url = $"/workspace/requests-component/{updated.Id}";

                    if (!string.IsNullOrEmpty(employee?.PushEndpoint))
                    {
                        string pushTitle = "✅ Task Status Updated!";
                        string pushBody = $"Room {updated.RoomNumber} status has changed to: '{updated.Status}'.";
                        string targetUrl = url;

                        try
                        {
                            await _notificationService.SendNotificationToEmployeeAsync(
                                employee.Id,
                                pushTitle,
                                pushBody,
                                targetUrl
                            );
                        }
                        catch (WebPush.WebPushException webPushEx)
                        {
                            Console.WriteLine($"[WebPush Exception] Status: {webPushEx.StatusCode} | Reason: {webPushEx.Message}");
                        }
                        catch (Exception pushEx)
                        {
                            Console.WriteLine($"[General Push Error]: {pushEx.Message}");
                        }
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
            if (query == null)
            {
                query = new HistoryQueryDto { Page = 1, PageSize = 6 };
            }

            try
            {
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
