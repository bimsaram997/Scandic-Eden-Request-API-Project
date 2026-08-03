namespace EdenRequest.Api.DTO
{
    public class UpdateExtraWorkDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public int? UpdatedById { get; set; }
    }
}
