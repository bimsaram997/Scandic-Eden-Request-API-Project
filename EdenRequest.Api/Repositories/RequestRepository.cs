using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IRequestRepository
    {
        Task<RequestHeader> CreateBulkAsync(RequestHeader header);
        Task<IEnumerable<RequestHeader>> GetAllRequetsAsync();
        Task<RequestHeader?> GetByIdAsync(int id);
        Task UpdateAsync(RequestHeader header);
        Task<PagedResponse<RequestHeader>> GetPagedRequestsByEmployeeAsync(int employeeId, bool isTeamLeader, HistoryQueryDto filters);
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

        public async Task<PagedResponse<RequestHeader>> GetPagedRequestsByEmployeeAsync(int employeeId, bool isTeamLeader, HistoryQueryDto filters)
        {
            var query = _context.RequestHeaders.AsQueryable();

            //  Role Guard: Housekeepers only see their own requests. Team Leaders can see everyone's.
            if (!isTeamLeader)
            {
                query = query.Where(h => h.EmployeeId == employeeId);
            }
            else if (filters.TargetEmployeeId.HasValue)
            {
                // Team leader selected a specific housekeeper from the dropdown name list
                query = query.Where(h => h.EmployeeId == filters.TargetEmployeeId.Value);
            }

            // Filter by exact List ID matching
            if (filters.RoomListId.HasValue)
            {
                query = query.Where(h => h.RoomListId == filters.RoomListId.Value);
            }

            //  Filter by Category (Checks if any material line item matches the Category ID)
            if (filters.CategoryId.HasValue)
            {
                query = query.Where(h => h.Lines.Any(l => l.Item.ItemCategoryId == filters.CategoryId.Value));
            }

            //  Filter by Specific Item ID Selection
            if (filters.ItemIds != null && filters.ItemIds.Any())
            {
                query = query.Where(h => h.Lines.Any(l => filters.ItemIds.Contains(l.ItemId)));
            }

            // Existing structural filters
            if (!string.IsNullOrEmpty(filters.RoomSearch))
            {
                query = query.Where(h => h.RoomNumber.Contains(filters.RoomSearch));
            }
            if (!string.IsNullOrEmpty(filters.Status) && !filters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                // Since your database handles strings, a direct match works perfectly in SQL
                query = query.Where(h => h.Status == filters.Status);
            }


            //  Combined Date & Time Timezone-Aware Parser (PostgreSQL ISO Native)
            if (filters.FromDate.HasValue)
            {
                // Extract the raw string date component: "2026-06-26"
                string dateStr = filters.FromDate.Value.ToString("yyyy-MM-dd");

                // Default to start of shift hour if not set
                string timeStr = !string.IsNullOrWhiteSpace(filters.FromTime) ? filters.FromTime : "00:00";

                // Create local text layout combined: "2026-06-26T08:20:00"
                string localIso = $"{dateStr}T{timeStr}:00";

                // Parse as Local, then convert to strict UTC timestamp safely
                DateTime localStart = DateTime.Parse(localIso);

                // Explicitly subtract your +3 Hour local offset (FLE Standard Time) to target exactly 05:20:00 UTC
                DateTime utcFrom = localStart.AddHours(-3);
                utcFrom = DateTime.SpecifyKind(utcFrom, DateTimeKind.Utc);

                query = query.Where(h => h.CreatedAt >= utcFrom);
            }

            if (filters.ToDate.HasValue)
            {
                string dateStr = filters.ToDate.Value.ToString("yyyy-MM-dd");
                string timeStr = !string.IsNullOrWhiteSpace(filters.ToTime) ? filters.ToTime : "23:59:59";

                string localIso = timeStr.Contains(":") && timeStr.Split(':').Length == 2
                    ? $"{dateStr}T{timeStr}:00"
                    : $"{dateStr}T{timeStr}";

                DateTime localEnd = DateTime.Parse(localIso);

                // Explicitly subtract your +3 Hour local offset here as well
                DateTime utcTo = localEnd.AddHours(-3);
                utcTo = DateTime.SpecifyKind(utcTo, DateTimeKind.Utc);

                query = query.Where(h => h.CreatedAt <= utcTo);
            }

            int totalCount = await query.CountAsync();
            var data = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Include(h => h.Employee)
                .Include(h => h.Lines).ThenInclude(l => l.Item).ThenInclude(i => i!.Category)
                .ToListAsync();

            return new PagedResponse<RequestHeader> { Data = data, TotalCount = totalCount };
        }

        public async Task UpdateAsync(RequestHeader header)
        {
            _context.RequestHeaders.Update(header);
            await _context.SaveChangesAsync();
        }
    }
}
