using System.ComponentModel.DataAnnotations;

namespace EdenRequest.Api.DTOs
{
    public class CreateExtraDirtyReportDto
    {
        public string RoomNumber { get; set; } = string.Empty;
     
        public int ReportedById { get; set; }

        public string? Notes { get; set; }

        [Required]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}