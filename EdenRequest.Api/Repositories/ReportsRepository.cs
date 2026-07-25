using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IReportsRepository
    {
        Task<List<RequestHeader>> GetHousekeeperSuppliesAsync(int housekeeperId);
        Task<List<ExtraWorkRequest>> GetHousekeeperExtraWorkAsync(int housekeeperId);

        //  NEW TEAM LEADER REPOSITORY METHODS
        Task<List<RequestHeader>> GetAllSuppliesAsync(DateTime startDate);
        Task<List<ExtraWorkRequest>> GetAllExtraWorkAsync(DateTime startDate);
    }
    public class ReportsRepository : IReportsRepository
    {
        private readonly AppDbContext _context;
        public ReportsRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<RequestHeader>> GetHousekeeperSuppliesAsync(int housekeeperId)
        {
            return await _context.RequestHeaders
                .Include(r => r.Lines)
                    .ThenInclude(l => l.Item)
                .Where(r => r.EmployeeId == housekeeperId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ExtraWorkRequest>> GetHousekeeperExtraWorkAsync(int housekeeperId)
        {
            return await _context.ExtraWorkRequests
                .Include(e => e.ExtraRequestLine)
                    .ThenInclude(l => l.ExtraWorkItem)
                .Where(e => e.AssignedToId == housekeeperId)
                .AsNoTracking()
                .ToListAsync();
        }

        //  TEAM LEADER IMPLEMENTATION
        public async Task<List<RequestHeader>> GetAllSuppliesAsync(DateTime startDate)
        {
            return await _context.RequestHeaders
                .Include(r => r.Employee)
                .Include(r => r.Lines).ThenInclude(l => l.Item)
                .Where(r => r.CreatedAt >= startDate)
                .AsNoTracking().ToListAsync();
        }

        public async Task<List<ExtraWorkRequest>> GetAllExtraWorkAsync(DateTime startDate)
        {
            return await _context.ExtraWorkRequests
                .Include(e => e.AssignedTo)
                .Include(e => e.RequestedBy)
                .Include(e => e.ExtraRequestLine).ThenInclude(l => l.ExtraWorkItem)
                .Where(e => e.AddedDate >= startDate)
                .AsNoTracking().ToListAsync();
        }
    }
}
