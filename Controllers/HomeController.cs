// 📁 Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using kutuphane.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace kutuphane.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        // Sınıf seviyesinde değişkeni tanımladık kanka, artık bağlamda var!
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            // Servisten asenkron olarak verileri çekip ViewBag'e aktarıyoruz
            var (totalBooks, totalMembers, activeLoans) = await _homeService.GetDashboardStatsAsync();

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalMembers = totalMembers;
            ViewBag.ActiveLoans = activeLoans;

            return View();
        }
    }
}