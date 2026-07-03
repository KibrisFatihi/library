using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;


namespace kutuphane.Controllers
{
    
    public class BookController : Controller
    {
        private readonly Supabase.Client _supabaseClient;

        public BookController(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Supabase'den silinmemiş (aktif) kitapları çekiyoruz
                var response = await _supabaseClient.From<Book>()
                    .Where(x => x.IsDeleted == false)
                    .Get();

                var kitaplar = response.Models;

                // Views/Book/Index.cshtml sayfasına verileri gönderiyoruz kanka
                return View(kitaplar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap listesi çekilirken hata oluştu: " + ex.Message);
                return View(new List<Book>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            try
            {
                // Konumları çekip ViewBag ile ekle sayfasına paslıyoruz
                var response = await _supabaseClient.From<BookLocation>().Get();
                var konumlar = response.Models;

                ViewBag.Konumlar = konumlar;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ekle sayfasında konumlar çekilemedi: " + ex.Message);
                ViewBag.Konumlar = new List<BookLocation>();
                return View();
            }
        }

        // 2. Formdan gelen yeni kitabı kaydeden fonksiyon (POST)
        [HttpPost]
        public async Task<IActionResult> Ekle(Book yeniKitap)
        {
            try
            {
                yeniKitap.IsDeleted = false; // Yeni kitap silinmemiş olarak başlar
                await _supabaseClient.From<Book>().Insert(yeniKitap);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap eklenirken hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 3. Yazılımsal Silme (Soft Delete) Fonksiyonu
        [HttpGet("Book/Sil/{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
                var mevcutKitap = response.Models.FirstOrDefault();

                if (mevcutKitap != null)
                {
                    mevcutKitap.IsDeleted = true;
                    await _supabaseClient.From<Book>().Update(mevcutKitap);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Soft Delete Hatası: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 4. Düzenleme sayfasını açan fonksiyon (GET)
        [HttpGet("Book/Duzenle/{id}")]
        public async Task<IActionResult> Duzenle(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
                var kitap = response.Models.FirstOrDefault();

                if (kitap == null)
                {
                    return RedirectToAction("Index");
                }

                // Düzenleme sayfasında da konumu değiştirebilmesi için konum listesini gönderiyoruz kanka
                var konumResponse = await _supabaseClient.From<BookLocation>().Get();
                ViewBag.Konumlar = konumResponse.Models;

                return View(kitap);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Düzenleme Sayfası Açılırken Hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 5. Formdan gelen güncel verileri Supabase'e kaydeden fonksiyon (POST)
        [HttpPost]
        public async Task<IActionResult> Duzenle(Book guncelKitap)
        {
            try
            {
                guncelKitap.IsDeleted = false;
                await _supabaseClient.From<Book>().Update(guncelKitap);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap Güncellenirken Hata Oluştu: " + ex.Message);
                return View(guncelKitap);
            }
        }

    }
}
