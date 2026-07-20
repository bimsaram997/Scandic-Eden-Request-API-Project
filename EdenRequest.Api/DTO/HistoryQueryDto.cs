namespace EdenRequest.Api.DTO
{
    public class HistoryQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        public string? RoomSearch { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? RequestedById { get; set; }   

        public int? RoomListId { get; set; }
        public int? CategoryId { get; set; }
        public List<int> ItemIds { get; set; } = new();
        public int? TargetEmployeeId { get; set; }

        public string? FromTime { get; set; }
        public string? ToTime { get; set; }
    }
}
