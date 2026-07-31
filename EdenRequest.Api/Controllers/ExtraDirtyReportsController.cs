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

        [HttpPost("metadata")]
        public async Task<IActionResult> CreateReportMetadata([FromBody] CreateReportMetadataDto dto)
        {
            try
            {
                int reportId = await _service.CreateReportMetadataAsync(dto);
                return Ok(new { reportId = reportId, message = "Report metadata created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.GetBaseException().Message });
            }
        }

        [HttpPost("{reportId:int}/media")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100 * 1024 * 1024)] // Allow up to 100MB for mobile uploads
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
        public async Task<IActionResult> UploadMedia(int reportId, [FromForm(Name = "files")] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "At least one media file is required." });
            }

            try
            {
                await _service.AttachMediaToReportAsync(reportId, files);
                return Ok(new { message = "Media uploaded successfully." });
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
