using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;
using kutuphane.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace kutuphane.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        private readonly BookLocationService _locationService;

        public BookController(BookService bookService, BookLocationService locationService)
        {
            _bookService = bookService;
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchString = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    ViewBag.CurrentFilter = searchString;
                    var rpcResult = await _bookService.SearchBooksAsync(searchString);
                    return View(rpcResult);
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
        public async Task<IActionResult> Ekle()
        {
            ViewBag.Konumlar = await _locationService.GetAllLocationsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Book yeniKitap)
        {
            await _bookService.AddBookAsync(yeniKitap);
            return RedirectToAction("Index");
        }

        [HttpGet("Book/Sil/{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            await _bookService.SoftDeleteBookAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet("Book/Duzenle/{id}")]
        public async Task<IActionResult> Duzenle(int id)
        {
            var kitap = await _bookService.GetBookByIdAsync(id);
            if (kitap == null) return RedirectToAction("Index");

            ViewBag.Konumlar = await _locationService.GetAllLocationsAsync();
            return View(kitap);
        }

        [HttpPost]
        public async Task<IActionResult> Duzenle(Book guncelKitap)
        {
            await _bookService.UpdateBookAsync(guncelKitap);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Arsiv()
        {
            var arsiv = await _bookService.GetArchivedBooksAsync();
            return View(arsiv);
        }

        [HttpGet("Book/GeriAl/{id}")]
        public async Task<IActionResult> GeriAl(int id)
        {
            await _bookService.RecoverBookFromArchiveAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet("Book/KaliciSil/{id}")]
        public async Task<IActionResult> KaliciSil(int id)
        {
            await _bookService.HardDeleteBookAsync(id);
            return RedirectToAction("Arsiv");
        }
    }
}