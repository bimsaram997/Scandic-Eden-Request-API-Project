namespace EdenRequest.Api.DTOs
{
    public class TeamLeaderKpiDto
    {
        public int TotalExtraWorkToday { get; set; }
        public int CompletedExtraWorkToday { get; set; }
        public int TotalSupplyRequestsToday { get; set; }
        public double AvgFulfillmentSpeedMinutes { get; set; }
        public int ActiveHousekeepersCount { get; set; }
    }
}