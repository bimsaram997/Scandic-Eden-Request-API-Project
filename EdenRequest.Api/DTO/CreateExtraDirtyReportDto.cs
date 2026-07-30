using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdenRequest.Api.DTOs
{
    public class CreateExtraDirtyReportDto
    {
        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public int ReportedById { get; set; }

        public string? Notes { get; set; }

        public List<IFormFile> Files { get; set; } = new();
    }
}