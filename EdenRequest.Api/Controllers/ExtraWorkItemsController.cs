using EdenRequest.Api.DTO;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraWorkItemsController : ControllerBase
    {
        private readonly IExtraWorkItemService _extraWorkItemService;

        public ExtraWorkItemsController(IExtraWorkItemService extraWorkItemService)
        {
            _extraWorkItemService = extraWorkItemService;
        }

        // 🔍 GET: api/ExtraWorkItems
        [HttpGet("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ExtraWorkItemDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ExtraWorkItemDto>>> GetAll()
        {
            try
            {
                IEnumerable<ExtraWorkItemDto> items = await _extraWorkItemService.GetAllExtraWorkItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error loading items: {ex.Message}" });
            }
        }

        // 🔍 GET: api/ExtraWorkItems/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraWorkItemDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ExtraWorkItemDto>> GetById(int id)
        {
            try
            {
                ExtraWorkItemDto item = await _extraWorkItemService.GetByIdAsync(id);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error looking up item: {ex.Message}" });
            }
        }

        //  POST: api/ExtraWorkItems
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ExtraWorkItemDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExtraWorkItemDto>> Create([FromBody] CreateExtraWorkDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ExtraWorkItemDto createdItem = await _extraWorkItemService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdItem.Id },
                    createdItem
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while saving the extra work item.", details = ex.Message });
            }
        }

        //  PUT: api/ExtraWorkItems/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExtraWorkDto dto)
        {
            // Defensive check to ensure the URL parameter matches the payload identifier
            if (id != dto.Id)
            {
                return BadRequest(new { message = "The route ID variation does not match the payload ID identity." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _extraWorkItemService.UpdateAsync(dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while updating the item.", details = ex.Message });
            }
        }
    }
}