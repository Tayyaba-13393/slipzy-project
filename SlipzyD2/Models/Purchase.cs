
using System.ComponentModel.DataAnnotations;

namespace SlipzyD2.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string StoreName { get; set; } = "";

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Range(0, 999999999)]
        public decimal TotalAmount { get; set; }

        public DateTime? WarrantyStart { get; set; }

        public int? WarrantyMonths { get; set; }

        public DateTime? WarrantyExpiry { get; set; }

        public DateTime? ReturnDeadline { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<PurchaseItem> Items { get; set; } = new();

        public string? ProductNames { get; set; }
        public List<ReceiptFile> Receipts { get; set; } = new();

        public string? ReceiptPath { get; set; }
    }
}
