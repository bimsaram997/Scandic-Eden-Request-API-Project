using Microsoft.AspNetCore.Http.Headers;
using System.Text.Json.Serialization;

namespace EdenRequest.Api.Data
{
    public class RequestHeader
    {
        public int Id { get; set; }
        public string? RoomNumber { get; set; }
        public bool IsGeneralRequest => string.IsNullOrWhiteSpace(RoomNumber);

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // New Tracking Audit Properties (OOP Relationships)
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int RoomListId { get; set; } // The active shift list ID used by the worker

        public List<RequestLine> Lines { get; set; } = new();
    }

    public class RequestLine
    {
        public int Id { get; set; }
        public int RequestHeaderId { get; set; }
        [JsonIgnore]
        public RequestHeader? RequestHeader { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }

        public int Quantity { get; set; }
        public string UnitType { get; set; } = "Pcs"; // "Pcs" or "Trolley"
    }
}
