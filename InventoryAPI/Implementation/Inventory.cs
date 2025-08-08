using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryAPI.Implementation
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
