using System.Text.Json.Serialization;

namespace EdenRequest.Api.Data
{
    public class ExtraWorkRequest
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public int ListNumber { get; set; } 
        public string Status { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public List<ExtraRequestLine> ExtraRequestLine { get; set; } = new();
        public string? Notes { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public int RequestedById { get; set; }
        public Employee? RequestedBy { get; set; }
        public int AssignedToId { get; set; }
        public Employee? AssignedTo { get; set; }
        public int? UpdatedById { get; set; }
        public Employee? UpdatedBy { get; set; }

    }

    public class ExtraRequestLine
    {
        public int Id { get; set; }
        public int ExtraWorkRequestId { get; set; }
        [JsonIgnore]
        public ExtraWorkRequest? ExtraWorkRequest { get; set; }
        public int ExtraWorkItemId { get; set; }
        public ExtraWorkItem? ExtraWorkItem { get; set; }

        public int Quantity { get; set; }
    }
}
