
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipzyD2.Repositories.Interfaces;
using System.Security.Claims;

namespace SlipzyD2.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _repository;

        public DashboardController(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var data = await _repository.GetDashboardDataAsync(userId);

            ViewBag.FullName = User.Identity?.Name ?? "there";

            return View(data);
        }
    }
}
