using EdenRequest.Api.Data;
using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
        {
            try
            {
                var item = await _itemService.CreateItemAsync(dto.Name, dto.CategoryId);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("itemsByCategoryId/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsByCategoryId(int categoryId)
        {
            try
            {
                var items = await _itemService.GetItemsByCategoryIdAsync(categoryId);

                
                return Ok(items);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }



    public record CreateItemDto(string Name, int CategoryId);

}
