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

        //The toggle flag from your frontend
        public bool IsToday { get; set; } = true;

        // Optional: Keep these in case you want custom range searches in the "Past" view
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
        public bool IsTeamLeader { get; set; }
    }
}
