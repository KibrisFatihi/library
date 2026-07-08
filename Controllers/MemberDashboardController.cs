// 📁 Controllers/MemberDashboardController.cs

using Kutuphane.Models;
using kutuphane.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kutuphane.Controllers
{
    [Authorize(Roles = "Member,Admin")]
    public class MemberDashboardController : Controller
    {
        private readonly BookService _bookService;
        private readonly LoanService _loanService;

        // Constructor adı sınıf adıyla birebir aynı kanka
        public MemberDashboardController(BookService bookService, LoanService loanService)
        {
            _bookService = bookService;
            _loanService = loanService;
        }

        // 🚨 ÜYENİN İLK GİRDİĞİ ANA PANEL (Giriş yapınca buraya düşer)
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.UserName = User.Identity?.Name;
            return View(); // Views/MemberDashboard/Index.cshtml sayfasını açar kanka
        }

        // 🚨 ÜYENİN KİTAPLARI LİSTELEDİĞİ AYRI METOT (Butona basınca burası tetiklenecek)
        [HttpGet]
        public async Task<IActionResult> Book(string? searchString = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    ViewBag.CurrentFilter = searchString;
                    var rpcResult = await _bookService.SearchBooksAsync(searchString);
                    return View(rpcResult); // Views/MemberDashboard/Books.cshtml sayfasını açar kanka
                }

                var kitaplar = await _bookService.GetActiveBooksAsync();
                return View(kitaplar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                return View(new List<Book>());
            }
        }
        [HttpGet]
        [Authorize(Roles = "Member,Admin")] // Güvenlik duvarını unutmayalım kanka
        public async Task<IActionResult> MyLoan()
        {
            

            // Geçici olarak NameIdentifier yerine listedeki ilk claim'i yakalamaya çalışalım:
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault()?.Value;
           
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "MemberDashboard");
            }
            var memberId = int.Parse(userId);
            var myLoans = await _loanService.GetMemberActiveLoansAsync(memberId);
            return View(myLoans);
        }
    }
}