
using Dapper;
using SlipzyD2.Data;
using SlipzyD2.Models;
using SlipzyD2.Repositories.Interfaces;

namespace SlipzyD2.Repositories
{
    public class PurchaseItemRepository : IPurchaseItemRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PurchaseItemRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(
            int purchaseId)
        {
            const string sql = @"
                SELECT PurchaseItemId, PurchaseId,
                       ProductName, Quantity, UnitPrice, LineTotal
                FROM dbo.PurchaseItems
                WHERE PurchaseId = @PurchaseId;";

            using var connection = _factory.CreateConnection();

            return await connection.QueryAsync<PurchaseItem>(
                sql, new { PurchaseId = purchaseId });
        }
    }
}
