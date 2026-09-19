namespace EdenRequest.Api.DTOs
{
    public class TeamLeaderReportDto
    {
        public TeamLeaderKpiDto Kpis { get; set; } = new();
        public List<HousekeeperPerformanceDto> StaffPerformance { get; set; } = new();
        public List<TopRequestedItemDto> TopItems { get; set; } = new();
        public List<MasterTaskLogDto> MasterTaskLog { get; set; } = new();
    }
}