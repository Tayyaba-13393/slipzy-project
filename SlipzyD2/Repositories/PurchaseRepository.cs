
using Dapper;
using System.Data;
using SlipzyD2.Data;
using SlipzyD2.Models;
using SlipzyD2.Repositories.Interfaces;

namespace SlipzyD2.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PurchaseRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> CreateAsync(
            Purchase purchase,
            List<PurchaseItem> items,
            ReceiptFile? receipt)
        {
            using var connection = _factory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string purchaseSql = @"
                    INSERT INTO dbo.Purchases
                    (
                        UserId, StoreName, PurchaseDate, CategoryId,
                        TotalAmount, WarrantyStart, WarrantyMonths,
                        WarrantyExpiry, ReturnDeadline
                    )
                    VALUES
                    (
                        @UserId, @StoreName, @PurchaseDate, @CategoryId,
                        @TotalAmount, @WarrantyStart, @WarrantyMonths,
                        @WarrantyExpiry, @ReturnDeadline
                    );

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int purchaseId = await connection.ExecuteScalarAsync<int>(
                    purchaseSql, purchase, transaction);

                const string itemSql = @"
                    INSERT INTO dbo.PurchaseItems
                    (PurchaseId, ProductName, Quantity, UnitPrice)
                    VALUES
                    (@PurchaseId, @ProductName, @Quantity, @UnitPrice);";

                foreach (var item in items)
                {
                    item.PurchaseId = purchaseId;

                    await connection.ExecuteAsync(
                        itemSql, item, transaction);
                }

                if (receipt != null)
                {
                    receipt.PurchaseId = purchaseId;

                    const string receiptSql = @"
                        INSERT INTO dbo.ReceiptFiles
                        (
                            PurchaseId, OriginalFileName, RelativePath
                        )
                        VALUES
                        (
                            @PurchaseId, @OriginalFileName, @RelativePath
                        );";

                    await connection.ExecuteAsync(
                        receiptSql, receipt, transaction);
                }

                transaction.Commit();

                return purchaseId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync(
            string userId,
            string? search,
            int? categoryId,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            const string sql = @"
                SELECT
                    p.*,
                    c.CategoryName,
                    (
                        SELECT STRING_AGG(pi.ProductName, ', ')
                        FROM dbo.PurchaseItems pi
                        WHERE pi.PurchaseId = p.PurchaseId
                    ) AS ProductNames
                FROM dbo.Purchases p
                INNER JOIN dbo.Categories c
                    ON p.CategoryId = c.CategoryId
                WHERE p.UserId = @UserId
                    AND (@Search IS NULL
                        OR p.StoreName LIKE '%' + @Search + '%'
                        OR EXISTS (
                            SELECT 1
                            FROM dbo.PurchaseItems pi
                            WHERE pi.PurchaseId = p.PurchaseId
                              AND pi.ProductName LIKE '%' + @Search + '%'
                        ))
                    AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    AND (@FromDate IS NULL OR p.PurchaseDate >= @FromDate)
                    AND (@ToDate IS NULL OR p.PurchaseDate < DATEADD(DAY, 1, @ToDate))
                ORDER BY p.PurchaseDate DESC, p.PurchaseId DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using var connection = _factory.CreateConnection();

            return await connection.QueryAsync<Purchase>(sql, new
            {
                UserId = userId,
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                CategoryId = categoryId,
                FromDate = fromDate,
                ToDate = toDate,
                Offset = (Math.Max(1, page) - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<int> CountAsync(
            string userId,
            string? search,
            int? categoryId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM dbo.Purchases p
                WHERE p.UserId = @UserId
                    AND (@Search IS NULL
                        OR p.StoreName LIKE '%' + @Search + '%'
                        OR EXISTS (
                            SELECT 1
                            FROM dbo.PurchaseItems pi
                            WHERE pi.PurchaseId = p.PurchaseId
                              AND pi.ProductName LIKE '%' + @Search + '%'
                        ))
                    AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    AND (@FromDate IS NULL OR p.PurchaseDate >= @FromDate)
                    AND (@ToDate IS NULL OR p.PurchaseDate < DATEADD(DAY, 1, @ToDate));";

            using var connection = _factory.CreateConnection();

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserId = userId,
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                CategoryId = categoryId,
                FromDate = fromDate,
                ToDate = toDate
            });
        }

        public async Task<Purchase?> GetByIdAsync(int id, string userId)
        {
            const string sql = @"
                SELECT p.*, c.CategoryName
                FROM dbo.Purchases p
                INNER JOIN dbo.Categories c
                    ON p.CategoryId = c.CategoryId
                WHERE p.PurchaseId = @Id AND p.UserId = @UserId;

                SELECT pi.*
                FROM dbo.PurchaseItems pi
                INNER JOIN dbo.Purchases p
                    ON pi.PurchaseId = p.PurchaseId
                WHERE p.PurchaseId = @Id AND p.UserId = @UserId;

                SELECT rf.*
                FROM dbo.ReceiptFiles rf
                INNER JOIN dbo.Purchases p
                    ON rf.PurchaseId = p.PurchaseId
                WHERE p.PurchaseId = @Id AND p.UserId = @UserId;";

            using var connection = _factory.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                sql, new { Id = id, UserId = userId });

            var purchase = await multi.ReadFirstOrDefaultAsync<Purchase>();

            if (purchase == null)
                return null;

            purchase.Items = (await multi.ReadAsync<PurchaseItem>()).ToList();

            purchase.Receipts = (await multi.ReadAsync<ReceiptFile>()).ToList();

            purchase.ReceiptPath = purchase.Receipts
                .FirstOrDefault()?.RelativePath;

            return purchase;
        }

        public async Task<bool> UpdateAsync(Purchase purchase)
        {
            const string sql = @"
                UPDATE dbo.Purchases
                SET StoreName = @StoreName,
                    PurchaseDate = @PurchaseDate,
                    CategoryId = @CategoryId,
                    TotalAmount = @TotalAmount,
                    WarrantyStart = @WarrantyStart,
                    WarrantyMonths = @WarrantyMonths,
                    WarrantyExpiry = @WarrantyExpiry,
                    ReturnDeadline = @ReturnDeadline
                WHERE PurchaseId = @PurchaseId
                  AND UserId = @UserId;";

            using var connection = _factory.CreateConnection();

            return await connection.ExecuteAsync(sql, purchase) > 0;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            const string sql = @"
                DELETE FROM dbo.Purchases
                WHERE PurchaseId = @Id AND UserId = @UserId;";

            using var connection = _factory.CreateConnection();

            return await connection.ExecuteAsync(sql, new
            {
                Id = id,
                UserId = userId
            }) > 0;
        }
    }
}
