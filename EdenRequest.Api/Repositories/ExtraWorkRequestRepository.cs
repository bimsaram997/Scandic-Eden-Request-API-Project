using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IExtraWorkRequestRepository
    {
        Task<ExtraWorkRequest> CreateBulkAsync(ExtraWorkRequest extraWorkRequest);
        Task<ExtraWorkRequest?> GetByIdAsync(int id);
        Task<PagedResponse<ExtraWorkRequest>> GetPagedExtraWorkRequestsAsync(AllExtraWorkQueryDto filters);
        Task UpdateStatusAsync(ExtraWorkRequest header);

    }
    public class ExtraWorkRequestRepository: IExtraWorkRequestRepository
    {
        private readonly AppDbContext _context;

        public ExtraWorkRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExtraWorkRequest> CreateBulkAsync(ExtraWorkRequest extraWorkRequest)
        {
            await _context.ExtraWorkRequests.AddAsync(extraWorkRequest);
            await _context.SaveChangesAsync();
            return extraWorkRequest;
        }

        public async Task<ExtraWorkRequest?> GetByIdAsync(int id)
        {
            return await _context.ExtraWorkRequests
                .Include(e => e.RequestedBy)    
                .Include(e => e.AssignedTo)       
                .Include(e => e.UpdatedBy)        
                .Include(e => e.ExtraRequestLine)
                    .ThenInclude(l => l.ExtraWorkItem)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task<PagedResponse<ExtraWorkRequest>> GetPagedExtraWorkRequestsAsync(AllExtraWorkQueryDto filters)
        {
            var query = _context.ExtraWorkRequests.AsQueryable();
            if (!filters.IsTeamLeader)
            {
                query = query.Where(r => r.AssignedToId == filters.AssignedToId);
            }
            else if (filters.AssignedToId.HasValue)
            {
                // Team leader selected a specific housekeeper from the dropdown name list
                query = query.Where(h => h.AssignedToId == filters.AssignedToId.Value);
            }

            // 1. Core Field Filters
            if (filters.RoomNumber != null)
            {
                query = query.Where(r => r.RoomNumber == filters.RoomNumber);
            }
            if (filters.RequestedById.HasValue)
            {
                query = query.Where(r => r.RequestedById == filters.RequestedById.Value);
            }
            if (filters.AssignedToId.HasValue)
            {
                query = query.Where(r => r.AssignedToId == filters.AssignedToId.Value);
            }
            if (!string.IsNullOrEmpty(filters.Status) && !filters.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status == filters.Status);
            }

            if (filters.ExtraItemIds != null && filters.ExtraItemIds.Any())
            {
                query = query.Where(r => r.ExtraRequestLine.Any(l => filters.ExtraItemIds.Contains(l.ExtraWorkItemId)));
            }

            if (filters.listNumber.HasValue)
            {
                query = query.Where(r => r.ListNumber == filters.listNumber.Value);
            }

            // 2. Today vs. Past Timezone-Aware Query Boundaries
            // 1. Get current time strictly in UTC
            DateTime utcNow = DateTime.UtcNow;

            // 2. Convert it to your local time (+3 Hours) to see what day it is for you
            DateTime localTime = utcNow.AddHours(3);

            // 3. Get the start of your local today (00:00:00)
            DateTime localStartOfToday = new DateTime(localTime.Year, localTime.Month, localTime.Day, 0, 0, 0);

            // 4. Shift local midnight back to UTC (-3 Hours) and mark it explicitly as Utc Kind
            DateTime utcTodayStart = DateTime.SpecifyKind(localStartOfToday.AddHours(-3), DateTimeKind.Utc);

            if (filters.IsToday)
            {
                // TODAY: Fetch items with timestamps starting from local midnight UTC boundary
                query = query.Where(r => r.AddedDate >= utcTodayStart);
            }
            else
            {
                // 🟢 FIX: Only apply the default "exclude today" rule if the user DID NOT send a custom date range!
                if (!filters.FromDate.HasValue && !filters.ToDate.HasValue)
                {
                    query = query.Where(r => r.AddedDate < utcTodayStart);
                }

                // Apply custom history filters if sent from the frontend
                if (filters.FromDate.HasValue)
                {
                    string dateStr = filters.FromDate.Value.ToString("yyyy-MM-dd");
                    string timeStr = !string.IsNullOrWhiteSpace(filters.FromTime) ? filters.FromTime : "00:00";
                    string localIso = $"{dateStr}T{timeStr}:00";

                    DateTime localStart = DateTime.Parse(localIso);
                    DateTime utcFrom = localStart.AddHours(-3); // Adjust to your local time zone offset
                    utcFrom = DateTime.SpecifyKind(utcFrom, DateTimeKind.Utc);

                    query = query.Where(h => h.AddedDate >= utcFrom);
                }

                if (filters.ToDate.HasValue)
                {
                    string dateStr = filters.ToDate.Value.ToString("yyyy-MM-dd");
                    string timeStr = !string.IsNullOrWhiteSpace(filters.ToTime) ? filters.ToTime : "23:59:59";

                    string localIso = timeStr.Contains(":") && timeStr.Split(':').Length == 2
                        ? $"{dateStr}T{timeStr}:00"
                        : $"{dateStr}T{timeStr}";

                    DateTime localEnd = DateTime.Parse(localIso);
                    DateTime utcTo = localEnd.AddHours(-3); // Adjust to your local time zone offset
                    utcTo = DateTime.SpecifyKind(utcTo, DateTimeKind.Utc);

                    query = query.Where(h => h.AddedDate <= utcTo);
                }
            }

            if (filters.FromDate.HasValue)
            {
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

                query = query.Where(h => h.AddedDate >= utcFrom);
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

                query = query.Where(h => h.AddedDate <= utcTo);
            }

            int totalCount = await query.CountAsync();

            var data = await query
                .AsNoTracking()
                .OrderByDescending(r => r.AddedDate)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
    
                .Include(r => r.RequestedBy)
                .Include(r => r.AssignedTo)
                .Include(r => r.UpdatedBy)
                .Include(r => r.ExtraRequestLine)
                    .ThenInclude(l => l.ExtraWorkItem)
                .ToListAsync();

            return new PagedResponse<ExtraWorkRequest>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
        public async Task UpdateStatusAsync(ExtraWorkRequest header)
        {
            _context.ExtraWorkRequests.Update(header);
            await _context.SaveChangesAsync();
        }
    }

}
