using EdenRequest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemCategoryController : ControllerBase
    {
        private readonly IitemCategoryService _itemCategoryService;

        public ItemCategoryController(IitemCategoryService itemCategoryService)
        {
            _itemCategoryService = itemCategoryService;
        }

        [HttpPost("getAll")]
        public async Task<IActionResult> GetAllItemCategories()
        {
            var categories = await _itemCategoryService.GetAlItemCategoryAsync();
            return Ok(categories);
        }
    }
}
