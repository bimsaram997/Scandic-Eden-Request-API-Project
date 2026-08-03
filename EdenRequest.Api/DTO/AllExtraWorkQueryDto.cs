namespace EdenRequest.Api.DTO
{
    public class AllExtraWorkQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        public string? RoomNumber { get; set; }
        public int? RequestedById { get; set; }
        public int? AssignedToId { get; set; }
        public string? Status { get; set; }
        public int? listNumber { get; set; }
        public List<int> ExtraItemIds { get; set; } = new();
        public bool IsToday { get; set; } = true;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public bool IsTeamLeader { get; set; }
    }
}
