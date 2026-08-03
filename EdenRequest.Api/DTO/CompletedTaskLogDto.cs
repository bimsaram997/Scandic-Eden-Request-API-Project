namespace EdenRequest.Api.DTO
{
    /// <summary>
    /// Detailed activity record for tasks completed during today's shift
    /// </summary>
    public class CompletedTaskLogDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Extra Work" or "Supply Request"
        public string Description { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }
}
