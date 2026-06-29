namespace EdenRequest.Api.DTO
{
    public class RequestHistoryDto
    {
        public int Id { get; set; }
        public string? RoomNumber { get; set; }
        public int RoomListId { get; set; }
        public string Status { get; set; } = string.Empty; // Converted from Enum to clean string
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }

        // 🍇 The Nested Lines: Contains the collection of items requested
        public List<ItemLineDto> Items { get; set; } = new();
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "Unassigned";

    }

}
