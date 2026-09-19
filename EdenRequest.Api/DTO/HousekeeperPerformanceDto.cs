namespace EdenRequest.Api.DTOs
{
    public class HousekeeperPerformanceDto
    {
        public int HousekeeperId { get; set; }
        public string HousekeeperName { get; set; } = string.Empty;
        public int CompletedExtraWork { get; set; }
        public int RequestedSupplies { get; set; }
    }
}