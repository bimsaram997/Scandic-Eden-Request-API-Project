using EdenRequest.Api.DTO;
using EdenRequest.Api.Dtos;
using EdenRequest.Api.DTOs;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraDirtyReportsController : ControllerBase
    {
        private readonly IExtraDirtyReportService _service;

        public ExtraDirtyReportsController(IExtraDirtyReportService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromForm] CreateExtraDirtyReportDto dto)
        {
            // Fallback manual read from Request.Form for WebKit/iOS edge cases
            if (dto.ReportedById == 0 && Request.Form.TryGetValue("reportedById", out var rawReportedBy))
            {
                int.TryParse(rawReportedBy.FirstOrDefault(), out int parsedId);
                dto.ReportedById = parsedId;
            }

            if (string.IsNullOrWhiteSpace(dto.RoomNumber) && Request.Form.TryGetValue("roomNumber", out var rawRoom))
            {
                dto.RoomNumber = rawRoom.FirstOrDefault() ?? string.Empty;
            }

            try
            {
                var result = await _service.CreateReportAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraDirtyReportDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExtraDirtyReportDto>> GetById(int id)
        {
            var result = await _service.GetReportByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Extra work request with ID {id} was not found." });
            }

            return Ok(result);
        }

        [HttpPost("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<ExtraDirtyReportDto>))]
        public async Task<ActionResult<PagedResponse<ExtraDirtyReportDto>>> GetAll([FromBody] AllExtraDirtyQueryDto query)
        {
            if (query == null)
            {
                query = new AllExtraDirtyQueryDto { Page = 1, PageSize = 6, IsToday = true };
            }

            try
            {
                var response = await _service.GetAllReportsAsync(query);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error getting filtered extra requests: {ex.Message}" });
            }
        }
    }
}
