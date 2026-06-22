using EdenRequest.Api.Data;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IItemService
    {
        Task<Item> CreateItemAsync(string name, int categoryId);
    }
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<Item> CreateItemAsync(string name, int categoryId)
        {
            // Validate category exists
            var category = await _itemRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {categoryId} does not exist.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Item name cannot be empty.");
            }
               

            // Create and save the new item
            var newItem = new Item
            {
                Name = name,
                ItemCategoryId = categoryId
            };

            return await _itemRepository.AddItemAsync(newItem);
        }

    }
}
