using EdenRequest.Api.Data;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IitemCategoryService
    {
        // Define methods for item category operations here
        Task<IEnumerable<ItemCategory>> GetAlItemCategoryAsync();
    }
    public class itemCategoryService: IitemCategoryService
    {
        private readonly IItemCategoryRepository _itemCategoryRepository;

        public itemCategoryService(IItemCategoryRepository itemCategoryRepository)
        {
            _itemCategoryRepository = itemCategoryRepository;
        }

        public async Task<IEnumerable<ItemCategory>> GetAlItemCategoryAsync()
        {
            return await _itemCategoryRepository.GetAlItemCategoryAsync();
        }
    }
}
