namespace EdenRequest.Api.DTO
{
    /// <summary>
    /// High-level KPI summary metrics for the active shift
    /// </summary>
    public class HousekeeperKpiDto
    {
        public int ExtraWorkCompletedToday { get; set; }
        public int SuppliesRequestedToday { get; set; }
        public double AvgSetupSpeedMinutes { get; set; }
        public int PendingAssignedTasks { get; set; }
    }
}
