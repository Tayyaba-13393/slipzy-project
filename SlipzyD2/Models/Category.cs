
using System.ComponentModel.DataAnnotations;

namespace SlipzyD2.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = "";

        public List<Purchase> Purchases { get; set; } = new();
    }
}
