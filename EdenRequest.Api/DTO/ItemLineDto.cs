namespace EdenRequest.Api.DTO
{
    // 🏷️ The Nested Package: Represents an individual item row inside that request
    public class ItemLineDto
    {
        public string ItemName { get; set; } = string.Empty; // Deep-fetched from l.Item.Name
        public int Quantity { get; set; }
        public string UnitType { get; set; } = string.Empty;
    }

}
