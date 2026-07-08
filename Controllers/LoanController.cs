using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;
using kutuphane.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;

namespace kutuphane.Controllers
{

    public class LoanController : Controller
    {
        private readonly LoanService _loanService;

        public LoanController(LoanService loanService)
        {
            _loanService = loanService;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var odünçler = await _loanService.GetActiveLoansAsync();
            return View(odünçler);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KitapVer()
        {
            // 1. Servisten form datalarını ve tuple olarak dönen yapıları çekiyoruz
            var (kitaplar, uyeler) = await _loanService.GetLoanFormDataAsync();

            ViewBag.Kitaplar = kitaplar;
            ViewBag.Uyeler = uyeler;

            // GET anında sayfa ilk açıldığında varsayılan olarak bugünün tarihini atayalım kanka, input boş kalmasın
            ViewBag.DueDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> KitapVer(Readingbook yeniOdunc)
        {
            // 🚨 Kitap ödünç alma işlem anında BorrowDate'i bugünün tarihi olarak biz setleyelim kanka, admin uğraşmasın:
            yeniOdunc.BorrowDate = DateTime.UtcNow;
            yeniOdunc.IsReturned = false;

            var hataMesaji = await _loanService.BorrowBookAsync(yeniOdunc);

            if (hataMesaji != null)
            {
                // 🚨 KANKA HATA DURUMUNDA: Sayfa hata verip yenilenirse, adminin seçtiği tarih inputta silinmesin diye 
                // gelen modelin içindeki DueDate değerini tekrar ViewBag'e basıyoruz!
                ViewBag.HataMesaji = hataMesaji;

                var (kitaplar, uyeler) = await _loanService.GetLoanFormDataAsync();
                ViewBag.Kitaplar = kitaplar;
                ViewBag.Uyeler = uyeler;

                // Seçilen tarihi form elementine geri besliyoruz:
                ViewBag.DueDate = yeniOdunc.DueDate?.ToString("yyyy-MM-dd");

                return View(yeniOdunc);
            }

            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ReturnBook(int id)
        {
            await _loanService.ReturnBookAsync(id);
            return RedirectToAction("Index");
        }


    }
}