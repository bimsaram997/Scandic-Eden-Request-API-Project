namespace EdenRequest.Api.DTO
{
    public class CreateExtraWorkRequestDto
    {
        public string RoomNumber { get; set; } 
        public int ListNumber { get; set; }
        public int RequestedById { get; set; }
        public int AssignedToId { get; set; }
        public string? Notes { get; set; }
        public List<CreateExtraRequestLineDto> Lines { get; set; } = new();
    }

    public class CreateExtraRequestLineDto
    {
        public int ExtraWorkItemId { get; set; }
        public int Quantity { get; set; }
    }
}
