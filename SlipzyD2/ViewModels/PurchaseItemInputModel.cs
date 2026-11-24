
using System.ComponentModel.DataAnnotations;

namespace SlipzyD2.ViewModels
{
    public class PurchaseItemInputModel
    {
        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = "";

        [Range(1, 100000)]
        public int Quantity { get; set; } = 1;

        [Range(0.01, 999999999)]
        public decimal UnitPrice { get; set; }
    }
}
