
using SlipzyD2.Models;

namespace SlipzyD2.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardData> GetDashboardDataAsync(string userId);
    }
}
