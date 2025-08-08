namespace OrderAPI.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public Product Product { get; set; }
        public Inventory Inventory { get; set; }
    }
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
    public class Inventory
    {
        public int InventoryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}