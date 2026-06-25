using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IItemRepository
    {
        Task<Item> AddItemAsync(Item item);
        Task<IEnumerable<Item>> GetItemsByCategoryIdAsync(int categoryId);
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

        public async Task<IEnumerable<Item>> GetItemsByCategoryIdAsync(int categoryId)
        {
            return await _context.Items
        .Include(i => i.Category) // Optional: Loads category details automatically
        .Where(i => i.ItemCategoryId == categoryId)
        .ToListAsync();
        }


    }
}
