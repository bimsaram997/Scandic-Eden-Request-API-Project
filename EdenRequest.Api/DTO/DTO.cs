namespace EdenRequest.Api.DTOs
{

    public class MasterTaskLogDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; 
        public string AssignedToOrRequestedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}