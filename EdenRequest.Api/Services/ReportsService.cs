using EdenRequest.Api.DTO;
using EdenRequest.Api.DTOs;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IReportsService
    {
        Task<HousekeeperReportDto> GetHousekeeperReportAsync(int housekeeperId);
        Task<TeamLeaderReportDto> GetTeamLeaderReportAsync();
    }

    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _reportsRepository;

        public ReportsService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }

        public async Task<HousekeeperReportDto> GetHousekeeperReportAsync(int housekeeperId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            var sevenDaysAgo = todayUtc.AddDays(-6);

            var supplies = await _reportsRepository.GetHousekeeperSuppliesAsync(housekeeperId);
            var extraWork = await _reportsRepository.GetHousekeeperExtraWorkAsync(housekeeperId);

           
            // 1. KPIS CALCULATIONS
     
            var suppliesRequestedToday = supplies.Count(r => r.CreatedAt >= todayUtc);

            var extraWorkDoneToday = extraWork.Count(e =>
                e.Status == "Done" && e.DoneDate.HasValue && e.DoneDate.Value >= todayUtc);

            var pendingExtraWorkCount = extraWork.Count(e =>
                e.Status == "Pending" || e.Status == "Acknowledged");

            // Setup speed calculation using AddedDate and DoneDate
            var completedTasks = extraWork
                .Where(e => e.Status == "Done" && e.DoneDate.HasValue)
                .ToList();

            double avgSpeedMinutes = completedTasks.Any()
                ? completedTasks.Average(e => (e.DoneDate!.Value - e.AddedDate).TotalMinutes)
                : 0;


            // 2. WEEKLY TREND (LAST 7 DAYS)
            var weeklyTrend = new List<WeeklyTrendDto>();
            for (int i = 0; i < 7; i++)
            {
                var currentDate = sevenDaysAgo.AddDays(i);
                weeklyTrend.Add(new WeeklyTrendDto
                {
                    DayName = currentDate.ToString("ddd"),
                    SuppliesRequested = supplies.Count(s => s.CreatedAt.Date == currentDate.Date),
                    ExtraWorkCompleted = extraWork.Count(e =>
                        e.Status == "Done" &&
                        e.DoneDate.HasValue &&
                        e.DoneDate.Value.Date == currentDate.Date)
                });
            }

            
            // 3. SHIFT ACTIVITY LOG (TODAY)
       
            var extraWorkLogs = extraWork
                .Where(e => e.Status == "Done" && e.DoneDate.HasValue && e.DoneDate.Value >= todayUtc)
                .Select(e => new CompletedTaskLogDto
                {
                    RoomNumber = e.RoomNumber,
                    Category = "Extra Work",
                    Description = e.ExtraRequestLine.Any()
                        ? string.Join(", ", e.ExtraRequestLine.Select(l => $"{l.Quantity}x {l.ExtraWorkItem?.Name ?? "Item"}"))
                        : (e.Notes ?? "Extra Bed Setup"),
                    CompletedAt = e.DoneDate!.Value
                }).ToList();

            var supplyLogs = supplies
                .Where(r => r.CreatedAt >= todayUtc)
                .Select(r => new CompletedTaskLogDto
                {
                    RoomNumber = r.RoomNumber ?? "General",
                    Category = "Supply Request",
                    Description = r.Lines.Any()
                        ? string.Join(", ", r.Lines.Select(l => $"{l.Quantity} {l.UnitType} {l.Item?.Name ?? "Item"}"))
                        : "Supply Request",
                    CompletedAt = r.CreatedAt
                }).ToList();

            var todayLogs = extraWorkLogs
                .Concat(supplyLogs)
                .OrderByDescending(x => x.CompletedAt)
                .ToList();

            return new HousekeeperReportDto
            {
                Kpis = new HousekeeperKpiDto
                {
                    ExtraWorkCompletedToday = extraWorkDoneToday,
                    SuppliesRequestedToday = suppliesRequestedToday,
                    AvgSetupSpeedMinutes = Math.Round(avgSpeedMinutes, 1),
                    PendingAssignedTasks = pendingExtraWorkCount
                },
                WeeklyTrend = weeklyTrend,
                TodayTaskLog = todayLogs
            };
        }
        public async Task<TeamLeaderReportDto> GetTeamLeaderReportAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;

            // Fetch last 30 days of data for leaderboard & item totals
            var thirtyDaysAgo = todayUtc.AddDays(-30);

            var allSupplies = await _reportsRepository.GetAllSuppliesAsync(thirtyDaysAgo);
            var allExtraWork = await _reportsRepository.GetAllExtraWorkAsync(thirtyDaysAgo);

            // 1. TODAY'S KPIS
            var suppliesToday = allSupplies.Where(s => s.CreatedAt >= todayUtc).ToList();
            var extraWorkToday = allExtraWork.Where(e => e.AddedDate >= todayUtc).ToList();

            int totalExtraWorkToday = extraWorkToday.Count;
            int completedExtraWorkToday = extraWorkToday.Count(e => e.Status == "Done");
            int totalSuppliesToday = suppliesToday.Count;

            var activeStaffIds = suppliesToday.Select(s => s.EmployeeId)
                .Union(extraWorkToday.Select(e => e.AssignedToId))
                .Where(id => id > 0)
                .Distinct()
                .Count();

            // Setup speed average
            var completedTasksToday = extraWorkToday
                .Where(e => e.Status == "Done" && e.DoneDate.HasValue)
                .ToList();

            double avgSpeed = completedTasksToday.Any()
                ? completedTasksToday.Average(e => (e.DoneDate!.Value - e.AddedDate).TotalMinutes)
                : 0;

            // 2. STAFF PERFORMANCE LEADERBOARD (LAST 30 DAYS)
            var topStaffPerformance = allExtraWork
           .Where(e => e.AssignedTo != null)
           .GroupBy(e => new { e.AssignedToId, e.AssignedTo!.Name })
           .Select(g => new HousekeeperPerformanceDto
           {
               HousekeeperId = g.Key.AssignedToId,
               HousekeeperName = g.Key.Name,
               CompletedExtraWork = g.Count(e => e.Status == "Done"),
               RequestedSupplies = allSupplies.Count(s => s.EmployeeId == g.Key.AssignedToId)
           })
           .OrderByDescending(p => p.CompletedExtraWork)
           .ThenByDescending(p => p.RequestedSupplies)
           .Take(10) // Strictly caps leaderboard to Top 10
           .ToList();

            // 3. TOP REQUESTED ITEMS (SUPPLIES CONSUMPTION)
            var topItems = allSupplies
                .SelectMany(s => s.Lines)
                .Where(l => l.Item != null)
                .GroupBy(l => l.Item!.Name)
                .Select(g => new TopRequestedItemDto
                {
                    ItemName = g.Key,
                    TotalQuantity = g.Sum(l => l.Quantity)
                })
                .OrderByDescending(i => i.TotalQuantity)
                .Take(5)
                .ToList();

            // 4. MASTER AUDIT LOG (TODAY)
            var extraWorkLogs = extraWorkToday.Select(e => new MasterTaskLogDto
            {
                Id = e.Id,
                RoomNumber = e.RoomNumber,
                Category = "Extra Work",
                AssignedToOrRequestedBy = e.AssignedTo?.Name ?? "Unassigned",
                Details = e.ExtraRequestLine.Any()
                    ? string.Join(", ", e.ExtraRequestLine.Select(l => $"{l.Quantity}x {l.ExtraWorkItem?.Name ?? "Item"}"))
                    : (e.Notes ?? "Extra Bed Setup"),
                Status = e.Status,
                Timestamp = e.DoneDate ?? e.AddedDate
            }).ToList();

            var supplyLogs = suppliesToday.Select(s => new MasterTaskLogDto
            {
                Id = s.Id,
                RoomNumber = s.RoomNumber ?? "General",
                Category = "Supply Request",
                AssignedToOrRequestedBy = s.Employee?.Name ?? "Housekeeper",
                Details = s.Lines.Any()
                    ? string.Join(", ", s.Lines.Select(l => $"{l.Quantity} {l.UnitType} {l.Item?.Name ?? "Item"}"))
                    : "Supply Order",
                Status = s.Status,
                Timestamp = s.CreatedAt
            }).ToList();

            var masterLog = extraWorkLogs
                .Concat(supplyLogs)
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            return new TeamLeaderReportDto
            {
                Kpis = new TeamLeaderKpiDto
                {
                    TotalExtraWorkToday = totalExtraWorkToday,
                    CompletedExtraWorkToday = completedExtraWorkToday,
                    TotalSupplyRequestsToday = totalSuppliesToday,
                    AvgFulfillmentSpeedMinutes = Math.Round(avgSpeed, 1),
                    ActiveHousekeepersCount = activeStaffIds
                },
                StaffPerformance = topStaffPerformance,
                TopItems = topItems,
                MasterTaskLog = masterLog
            };
        }

    }
}
