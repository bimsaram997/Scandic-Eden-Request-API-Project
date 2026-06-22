namespace EdenRequest.Api.Data
{
    public class ItemCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Linen", "Glasses"
        public List<Item> Items { get; set; } = new();
    }
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Wine Glass", "Big Bed Sheet"
        public int ItemCategoryId { get; set; }
        public ItemCategory? Category { get; set; }
    }
}
