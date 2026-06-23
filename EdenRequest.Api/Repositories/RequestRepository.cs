using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IRequestRepository
    {
        Task<RequestHeader> CreateBulkAsync(RequestHeader header);
        Task<IEnumerable<RequestHeader>> GetAllRequetsAsync();
        Task<RequestHeader?> GetByIdAsync(int id);
        Task UpdateAsync(RequestHeader header);
    }
    public class RequestRepository: IRequestRepository
    {
        private readonly AppDbContext _context;

        public RequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RequestHeader> CreateBulkAsync(RequestHeader header)
        {
            await _context.RequestHeaders.AddAsync(header);
            await _context.SaveChangesAsync();
            return header;
        }

        public async Task<IEnumerable<RequestHeader>> GetAllRequetsAsync()
        {
            // Eagerly loading Employee and Items in one round-trip to the database

            return await _context.RequestHeaders
            .Include(h => h.Employee)
            .Include(h => h.Lines)
                .ThenInclude(l => l.Item)
                    .ThenInclude(i => i!.Category) 
            .ToListAsync();
        }

        public async Task<RequestHeader?> GetByIdAsync(int id)
        {
            // Eagerly loading Employee, Lines, Items, and Categories for a single record match
            return await _context.RequestHeaders
                .Include(h => h.Employee)
                .Include(h => h.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i!.Category) // This ensures Category isn't null here either!
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task UpdateAsync(RequestHeader header)
        {
            _context.RequestHeaders.Update(header);
            await _context.SaveChangesAsync();
        }
    }
}
