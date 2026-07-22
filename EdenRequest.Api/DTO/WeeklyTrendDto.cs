namespace EdenRequest.Api.DTO
{
    /// <summary>
    /// Represents daily output counts for the last 7 days
    /// </summary>
    public class WeeklyTrendDto
    {
        public string DayName { get; set; } = string.Empty; // e.g., "Mon", "Tue"
        public int SuppliesRequested { get; set; }
        public int ExtraWorkCompleted { get; set; }
    }
}
