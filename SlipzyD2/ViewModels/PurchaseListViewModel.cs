
using SlipzyD2.Models;

namespace SlipzyD2.ViewModels
{
    public class PurchaseListViewModel
    {
        public List<Purchase> Purchases { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 8;

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
