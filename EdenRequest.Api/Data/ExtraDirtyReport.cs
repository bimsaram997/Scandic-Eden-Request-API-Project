using System.Collections.Generic;

namespace EdenRequest.Api.Data
{
    public class ExtraDirtyReport
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int ReportedById { get; set; }
        public Employee? ReportedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
    }
    public class MediaFile
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; 
        public int ExtraDirtyReportId { get; set; }
    }
}
