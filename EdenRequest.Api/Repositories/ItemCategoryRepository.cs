using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IItemCategoryRepository
    {
        // Define methods for item category operations here
        Task<IEnumerable<ItemCategory>> GetAlItemCategoryAsync();
        Task<ItemCategory?> GetCategoryByIdAsync(int categoryId);
    }
    public class ItemCategoryRepository: IItemCategoryRepository
    {
        private readonly AppDbContext _context;

        public ItemCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemCategory>> GetAlItemCategoryAsync()
        {
            return await _context.ItemCategories.ToListAsync();
        }

        public async Task<ItemCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.ItemCategories.FindAsync(categoryId);
        }
    }
}
