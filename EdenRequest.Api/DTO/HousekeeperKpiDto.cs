namespace EdenRequest.Api.DTO
{
    public class HousekeeperKpiDto
    {
        public int ExtraWorkCompletedToday { get; set; }
        public int SuppliesRequestedToday { get; set; }
        public double AvgSetupSpeedMinutes { get; set; }
        public int PendingAssignedTasks { get; set; }
    }
}
