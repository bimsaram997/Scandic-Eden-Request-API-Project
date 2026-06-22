using EdenRequest.Api.Data;

namespace EdenRequest.Api.Repositories
{
    public interface IItemRepository
    {
        Task<Item> AddItemAsync(Item item);
        Task<ItemCategory?> GetCategoryByIdAsync(int categoryId);
    }

    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Item> AddItemAsync(Item item)
        {
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<ItemCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.ItemCategories.FindAsync(categoryId);
        }
    }
}
