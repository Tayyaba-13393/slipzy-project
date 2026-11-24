
using SlipzyD2.Models;

namespace SlipzyD2.Repositories.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<int> CreateAsync(
            Purchase purchase,
            List<PurchaseItem> items,
            ReceiptFile? receipt);

        Task<IEnumerable<Purchase>> GetAllAsync(
            string userId,
            string? search,
            int? categoryId,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize);

        Task<int> CountAsync(
            string userId,
            string? search,
            int? categoryId,
            DateTime? fromDate,
            DateTime? toDate);

        Task<Purchase?> GetByIdAsync(int id, string userId);

        Task<bool> DeleteAsync(int id, string userId);

        Task<bool> UpdateAsync(Purchase purchase);
    }
}
