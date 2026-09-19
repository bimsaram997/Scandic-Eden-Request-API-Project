namespace EdenRequest.Api.DTO
{
    public class ExtraWorkItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
    }
}
