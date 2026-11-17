using Microsoft.AspNetCore.Mvc;
using SlipzyD2.Models;
using System.Diagnostics;

namespace SlipzyD2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Splash()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
