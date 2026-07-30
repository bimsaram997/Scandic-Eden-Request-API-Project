using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdenRequest.Api.DTOs
{
    public class CreateExtraDirtyReportDto
    {
        [Required(ErrorMessage = "Room number is required.")]
        [FromForm(Name = "roomNumber")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReportedById is required.")]
        [FromForm(Name = "reportedById")]
        public int ReportedById { get; set; }

        [FromForm(Name = "notes")]
        public string? Notes { get; set; }

        [FromForm(Name = "files")]
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}