namespace EdenRequest.Api.Dtos
{
    public class ExtraWorkRequestDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public int ListNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RequestedById { get; set; }
        public string RequestedByEmployee { get; set; } = string.Empty; // e.g. "John Doe"

        public int AssignedToId { get; set; }
        public string AssignedToEmployee { get; set; } = "Unassigned"; // e.g. "Jane Smith"

        public int? UpdatedById { get; set; }
        public string UpdatedByEmployee { get; set; } = "N/A";

        public DateTime AddedDate { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public string? Notes { get; set; }

        // Lines now include the name of each item (e.g. "Bottle Warmer")
        public List<ExtraRequestLineDto> Lines { get; set; } = new();
    }

    public class ExtraRequestLineDto
    {
        public int Id { get; set; }
        public int ExtraWorkItemId { get; set; }
        public string ExtraWorkItemName { get; set; } = string.Empty; // Displays "Kettle", "Heater", etc.
        public int Quantity { get; set; }
    }
}