namespace EdenRequest.Api.DTOs
{
    public class AllExtraDirtyQueryDto
    {
        public bool IsTeamLeader { get; set; } = false;
        public int? ReportedById { get; set; }
        public string? RoomNumber { get; set; }
        public bool IsToday { get; set; } = true;

        // Custom date range filters
        public DateTime? FromDate { get; set; }
        public string? FromTime { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ToTime { get; set; }

        // Pagination parameters
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}