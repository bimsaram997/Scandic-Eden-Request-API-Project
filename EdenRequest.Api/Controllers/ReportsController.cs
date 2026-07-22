using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("housekeeper/{housekeeperId}")]
        public async Task<IActionResult> GetHousekeeperReport(int housekeeperId)
        {
            var report = await _reportsService.GetHousekeeperReportAsync(housekeeperId);
            return Ok(report);
        }
    }
}