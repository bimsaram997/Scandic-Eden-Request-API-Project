using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IExtraWorkItemRepository
    {
        Task<IEnumerable<ExtraWorkItem>> GetAllAsync();
        Task<ExtraWorkItem?> GetByIdAsync(int id);
        Task<ExtraWorkItem> CreateAsync(ExtraWorkItem item);
        Task UpdateAsync(ExtraWorkItem item);
    }
    public class ExtraWorkItemRepository: IExtraWorkItemRepository
    {
        private readonly AppDbContext _context;
        public ExtraWorkItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExtraWorkItem>> GetAllAsync()
        {
            return await _context.ExtraWorkItems.ToListAsync();
        }

        public async Task<ExtraWorkItem?> GetByIdAsync(int id)
        {
            return await _context.ExtraWorkItems.FindAsync(id);
        }
        public async Task<ExtraWorkItem> CreateAsync(ExtraWorkItem item)
        {
            await _context.ExtraWorkItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task UpdateAsync(ExtraWorkItem item)
        {
           _context.ExtraWorkItems.Update(item);
            await _context.SaveChangesAsync();
           
        }

    }
}
