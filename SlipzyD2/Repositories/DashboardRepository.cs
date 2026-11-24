
using Dapper;
using SlipzyD2.Data;
using SlipzyD2.Models;
using SlipzyD2.Repositories.Interfaces;

namespace SlipzyD2.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbConnectionFactory _factory;

        public DashboardRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<DashboardData> GetDashboardDataAsync(string userId)
        {
            const string summarySql = @"
                SELECT
                    COUNT(*) AS TotalPurchases,
                    COALESCE(SUM(TotalAmount), 0) AS TotalSpent,
                    COALESCE(SUM(
                        CASE WHEN MONTH(PurchaseDate) = MONTH(GETDATE())
                         AND YEAR(PurchaseDate) = YEAR(GETDATE())
                         THEN TotalAmount ELSE 0 END
                    ), 0) AS MonthlySpent,
                    SUM(CASE
                        WHEN (ReturnDeadline BETWEEN CAST(GETDATE() AS DATE)
                             AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE)))
                          OR (WarrantyExpiry BETWEEN CAST(GETDATE() AS DATE)
                             AND DATEADD(DAY, 30, CAST(GETDATE() AS DATE)))
                        THEN 1 ELSE 0 END) AS UpcomingDeadlines
                FROM dbo.Purchases
                WHERE UserId = @UserId;

                SELECT TOP 4
                    p.PurchaseId, p.StoreName, p.PurchaseDate,
                    p.TotalAmount, p.WarrantyExpiry, p.ReturnDeadline,
                    c.CategoryName,
                    (SELECT STRING_AGG(pi.ProductName, ', ')
                     FROM dbo.PurchaseItems pi
                     WHERE pi.PurchaseId = p.PurchaseId) AS ProductNames
                FROM dbo.Purchases p
                INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
                WHERE p.UserId = @UserId
                ORDER BY p.PurchaseDate DESC;

                SELECT TOP 5
                    p.PurchaseId, p.StoreName, p.WarrantyExpiry,
                    p.ReturnDeadline, p.PurchaseDate, p.TotalAmount
                FROM dbo.Purchases p
                WHERE p.UserId = @UserId
                  AND (
                    p.WarrantyExpiry IS NOT NULL
                    OR p.ReturnDeadline IS NOT NULL
                  )
                ORDER BY
                    CASE
                      WHEN p.ReturnDeadline IS NOT NULL
                       AND p.ReturnDeadline >= CAST(GETDATE() AS DATE)
                      THEN p.ReturnDeadline
                      ELSE p.WarrantyExpiry
                    END;

                SELECT c.CategoryName,
                       SUM(p.TotalAmount) AS Amount
                FROM dbo.Purchases p
                INNER JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
                WHERE p.UserId = @UserId
                GROUP BY c.CategoryName
                ORDER BY Amount DESC;

                SELECT TOP 1 rf.RelativePath
                FROM dbo.ReceiptFiles rf
                INNER JOIN dbo.Purchases p ON p.PurchaseId = rf.PurchaseId
                WHERE p.UserId = @UserId
                ORDER BY rf.UploadedAt DESC;";

            using var connection = _factory.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                summarySql, new { UserId = userId });

            var data = await multi.ReadSingleAsync<DashboardData>();

            data.RecentPurchases =
                (await multi.ReadAsync<Purchase>()).ToList();

            data.WarrantyPurchases =
                (await multi.ReadAsync<Purchase>()).ToList();

            data.CategorySpending =
    (await multi.ReadAsync<CategorySpending>())
    .ToList();

            data.LatestReceiptPath =
                await multi.ReadFirstOrDefaultAsync<string>();

            return data;
        }
    }
}
