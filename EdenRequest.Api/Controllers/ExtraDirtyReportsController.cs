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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.CreateReportAsync(dto);
                return Ok(new
                {
                    message = "Extra dirty report submitted successfully!",
                    reportId = result.Id,
                    uploadedFilesCount = result.MediaFiles?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                // 💡 UNWRAP INNER EXCEPTION: Shows the exact database constraint or column error
                var realErrorMessage = ex.GetBaseException().Message;
                return StatusCode(500, new { message = realErrorMessage });
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
