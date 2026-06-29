using EdenRequest.Api.Services;
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

        [HttpPost("{email}/{password}")]
        public async Task<IActionResult> GetEmployeeByEmailAndPassword(string email, string password)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByEmailAndPassword(email, password);
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

    }
}
