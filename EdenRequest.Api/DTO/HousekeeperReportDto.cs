namespace EdenRequest.Api.DTO
{
    public class HousekeeperReportDto
    {
        public HousekeeperKpiDto Kpis { get; set; } = new();
        public List<WeeklyTrendDto> WeeklyTrend { get; set; } = new();
        public List<CompletedTaskLogDto> TodayTaskLog { get; set; } = new();
    }
}
