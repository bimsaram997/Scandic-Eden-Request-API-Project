using System.ComponentModel.DataAnnotations;

namespace EdenRequest.Api.DTOs
{
    public class CreateReportMetadataDto
    {
        [Required(ErrorMessage = "Room number is required.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReportedById is required.")]
        public int ReportedById { get; set; }

        public string? Notes { get; set; }
    }
}