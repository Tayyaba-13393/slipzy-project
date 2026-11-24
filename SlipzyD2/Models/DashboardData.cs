
namespace SlipzyD2.Models
{
    public class DashboardData
    {
        public int TotalPurchases { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal MonthlySpent { get; set; }

        public int UpcomingDeadlines { get; set; }

        public List<Purchase> RecentPurchases { get; set; } = new();

        public List<Purchase> WarrantyPurchases { get; set; } = new();

        public List<CategorySpending> CategorySpending { get; set; } = new();
        public string? LatestReceiptPath { get; set; }
    }

    public class CategorySpending
    {
        public string CategoryName { get; set; } = "";

        public decimal Amount { get; set; }
    }
}
