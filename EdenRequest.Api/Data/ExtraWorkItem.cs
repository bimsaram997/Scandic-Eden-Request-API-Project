namespace EdenRequest.Api.Data
{
    public class ExtraWorkItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
