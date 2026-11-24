
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SlipzyD2.ViewModels
{
    public class CreatePurchaseViewModel
    {
        [Required]
        [StringLength(150)]
        public string StoreName { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required]
        public int CategoryId { get; set; }

        public DateTime? WarrantyStart { get; set; }

        [Range(0, 1200)]
        public int? WarrantyMonths { get; set; }

        public DateTime? ReturnDeadline { get; set; }

        public List<PurchaseItemInputModel> Items { get; set; } = new()
        {
            new PurchaseItemInputModel()
        };

        public IFormFile? Receipt { get; set; }
    }
}
