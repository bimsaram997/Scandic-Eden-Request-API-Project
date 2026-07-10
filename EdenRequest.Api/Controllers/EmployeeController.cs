using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeByEmailAndPassword([FromBody] LoginRequestModel request) // 🚀 FromBody!
        {
            try
            {
                
                var employee = await _employeeService.GetEmployeeByEmailAndPassword(request.Email, request.Password);
                return Ok(employee);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllEmployee")]
        public async Task<IActionResult> GetAllEmployeeByIdAsync()
        {
            var employees = await _employeeService.GetAllEmployeeAsync();
            return Ok(employees);
        }

        [HttpPut("{id}/push-token")]
        public async Task<IActionResult> SavePushToken(int id, [FromBody] PushSubscriptionDto dto)
        {
            var isSaved = await _employeeService.SavePushTokenAsync(id, dto);

            if (!isSaved)
            {
                return NotFound($"Employee with ID {id} not found.");
            }

            return NoContent(); 
        }

    }
}
