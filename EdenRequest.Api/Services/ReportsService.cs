using EdenRequest.Api.DTO;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IReportsService
    {
        Task<HousekeeperReportDto> GetHousekeeperReportAsync(int housekeeperId);
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

            // ----------------------------------------------------
            // 2. WEEKLY TREND (LAST 7 DAYS)
            // ----------------------------------------------------
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
    }
}
