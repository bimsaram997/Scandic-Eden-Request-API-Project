namespace EdenRequest.Api.DTOs
{
    public class ExtraDirtyReportDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int ReportedById { get; set; }
        public string ReportedByEmployee { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public System.Collections.Generic.List<MediaFileDto> MediaFiles { get; set; } = new System.Collections.Generic.List<MediaFileDto>();
    }
}