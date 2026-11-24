
using SlipzyD2.Models;

namespace SlipzyD2.Repositories.Interfaces
{
    public interface IPurchaseItemRepository
    {
        Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(int purchaseId);
    }
}
