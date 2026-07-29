using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IExtraDirtyReportRepository
    {
        Task<ExtraDirtyReport> AddReportAsync(ExtraDirtyReport report);
        Task<ExtraDirtyReport?> GetByIdAsync(int id);
        Task<PagedResponse<ExtraDirtyReport>> GetPagedReportsAsync(AllExtraDirtyQueryDto filters);
    }
    public class ExtraDirtyReportRepository : IExtraDirtyReportRepository
    {
        private readonly AppDbContext _context;

        public ExtraDirtyReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExtraDirtyReport> AddReportAsync(ExtraDirtyReport report)
        {
            await _context.ExtraDirtyReports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<ExtraDirtyReport?> GetByIdAsync(int id)
        {
            return await _context.ExtraDirtyReports
                .Include(r => r.ReportedBy)
                .Include(r => r.MediaFiles)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PagedResponse<ExtraDirtyReport>> GetPagedReportsAsync(AllExtraDirtyQueryDto filters)
        {
            var query = _context.ExtraDirtyReports.AsQueryable();

            // 1. Role / User Filters
            if (!filters.IsTeamLeader)
            {
                // Regular Employee: ALWAYS restrict to their own records
                if (filters.ReportedById.HasValue)
                {
                    query = query.Where(r => r.ReportedById == filters.ReportedById.Value);
                }
            }
            else
            {
                // Team Leader: OPTIONALLY filter by selected employee if provided
                if (filters.ReportedById.HasValue)
                {
                    query = query.Where(r => r.ReportedById == filters.ReportedById.Value);
                }
                // If ReportedById is null, it skips the filter and returns ALL records
            }
            // 2. Room Number Filter
            if (!string.IsNullOrEmpty(filters.RoomNumber))
            {
                query = query.Where(r => r.RoomNumber == filters.RoomNumber);
            }

            // Define UTC offset constant (+3 hours)
            TimeSpan localOffset = TimeSpan.FromHours(3);
            DateTime utcNow = DateTime.UtcNow;
            DateTime localTime = utcNow.Add(localOffset);

            // Start of today in UTC (+3 offset considered)
            DateTime localStartOfToday = localTime.Date; // Sets time to 00:00:00
            DateTime utcTodayStart = DateTime.SpecifyKind(localStartOfToday.Subtract(localOffset), DateTimeKind.Utc);

            // 1. Apply "IsToday" logic as default window if custom dates aren't set
            if (filters.IsToday && !filters.FromDate.HasValue && !filters.ToDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= utcTodayStart);
            }
            // 2. Default fallback: If NOT "IsToday" and NO dates provided, show past records (before today)
            else if (!filters.IsToday && !filters.FromDate.HasValue && !filters.ToDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt < utcTodayStart);
            }

            // 3. Custom FromDate Filter (Works with or without IsToday)
            if (filters.FromDate.HasValue)
            {
                string dateStr = filters.FromDate.Value.ToString("yyyy-MM-dd");
                string timeStr = !string.IsNullOrWhiteSpace(filters.FromTime) ? filters.FromTime : "00:00";

                // Format to HH:mm:00 if needed
                if (timeStr.Split(':').Length == 2)
                    timeStr += ":00";

                DateTime localStart = DateTime.Parse($"{dateStr}T{timeStr}");
                DateTime utcFrom = DateTime.SpecifyKind(localStart.Subtract(localOffset), DateTimeKind.Utc);

                query = query.Where(r => r.CreatedAt >= utcFrom);
            }

            // 4. Custom ToDate Filter (Works with or without IsToday)
            if (filters.ToDate.HasValue)
            {
                string dateStr = filters.ToDate.Value.ToString("yyyy-MM-dd");
                string timeStr = !string.IsNullOrWhiteSpace(filters.ToTime) ? filters.ToTime : "23:59:59";

                if (timeStr.Split(':').Length == 2)
                    timeStr += ":00";

                DateTime localEnd = DateTime.Parse($"{dateStr}T{timeStr}");
                DateTime utcTo = DateTime.SpecifyKind(localEnd.Subtract(localOffset), DateTimeKind.Utc);

                query = query.Where(r => r.CreatedAt <= utcTo);
            }

            int totalCount = await query.CountAsync();

            var data = await query
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Include(r => r.ReportedBy)
                .Include(r => r.MediaFiles)
                .ToListAsync();

            return new PagedResponse<ExtraDirtyReport>
            {
                Data = data,
                TotalCount = totalCount
            };
        }
    }
}
