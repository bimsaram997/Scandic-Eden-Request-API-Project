namespace EdenRequest.Api.DTOs
{
    public class MediaFileDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; 
    }
}