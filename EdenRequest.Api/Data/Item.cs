using System.Text.Json.Serialization;

namespace EdenRequest.Api.Data
{
    public class ItemCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        [JsonIgnore]
        public List<Item> Items { get; set; } = new();
    }
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemCategoryId { get; set; }
        public ItemCategory? Category { get; set; }
    }
}
