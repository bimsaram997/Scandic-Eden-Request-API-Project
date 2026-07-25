namespace EdenRequest.Api.DTOs
{
    public class TopRequestedItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }
}