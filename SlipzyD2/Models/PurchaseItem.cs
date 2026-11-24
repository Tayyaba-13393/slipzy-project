
using System.ComponentModel.DataAnnotations;

namespace SlipzyD2.Models
{
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }

        public int PurchaseId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = "";

        [Range(1, 100000)]
        public int Quantity { get; set; }

        [Range(0.01, 999999999)]
        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}
