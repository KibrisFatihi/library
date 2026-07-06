using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;
using Postgrest;
using static Postgrest.Constants;

namespace kutuphane.Controllers
{
    public class BookController : Controller
    {
        private readonly Supabase.Client _supabaseClient;

        public BookController(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        // 1. KİTAP LİSTELEME VE ARAMA (GET)
        [HttpGet]
        public async Task<IActionResult> Index(string searchString = null)
        {
            try
            {
                // Eğer arama kutusu doluysa doğrudan SQL Fonksiyonunu tetikliyoruz kanka
                if (!string.IsNullOrEmpty(searchString))
                {
                    string arananKelime = searchString.Trim();

                    var rpcResponse = await _supabaseClient.Rpc<List<Book>>("ara_kitaplar", new Dictionary<string, object>
                    {
                        { "aranan", arananKelime }
                    });

                    ViewBag.CurrentFilter = searchString;

                    // RPC'den gelen liste verisini doğrudan sayfaya gönderiyoruz kanka
                    return View(rpcResponse);
                }

                // Arama yapılmadıysa veritabanından id'ye göre büyükten küçüğe sıralı çekiyoruz kanka
                var response = await _supabaseClient.From<Book>()
                    .Where(x => x.IsDeleted == false)
                    .Order(x => x.BookId, Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var kitaplar = response.Models;
                return View(kitaplar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitaplar listelenirken arama hatası oluştu kanka: " + ex.Message);
                return View(new List<Book>());
            }
        }

        // 2. YENİ KİTAP EKLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            try
            {
                // KANKA DÜZELTME: BookLocation yerine yeni temiz modelimiz olan Location'ı çağırıyoruz
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

        // 3. YENİ KİTAP KAYDETME (POST)
        [HttpPost]
        public async Task<IActionResult> Ekle(Book yeniKitap)
        {
            try
            {
                yeniKitap.IsDeleted = false;
                await _supabaseClient.From<Book>().Insert(yeniKitap);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap eklenirken hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 4. SOFT DELETE (YAZILIMSAL SİLME)
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

        // 5. KİTAP DÜZENLEME SAYFASI (GET)
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

                // KANKA DÜZELTME: Buradaki BookLocation alanını da Location olarak güncelledik
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

        // 6. KİTAP GÜNCELLEME (POST)
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

        // 7. ARŞİVLENEN KİTAPLAR (GET)
        [HttpGet]
        public async Task<IActionResult> Arsiv()
        {
            try
            {
                var response = await _supabaseClient.From<Book>()
                    .Where(x => x.IsDeleted == true)
                    .Get();
                var arsivKitaplar = response.Models;
                return View(arsivKitaplar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Arşiv kitap listesi çekilirken hata oluştu: " + ex.Message);
                return View(new List<Book>());
            }
        }

        // 8. ARŞİVDEN GERİ ALMA
        [HttpGet("Book/GeriAl/{id}")]
        public async Task<IActionResult> GeriAl(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
                var mevcutKitap = response.Models.FirstOrDefault();
                if (mevcutKitap != null)
                {
                    mevcutKitap.IsDeleted = false;
                    await _supabaseClient.From<Book>().Update(mevcutKitap);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Geri Al Hatası: " + ex.Message);
                return RedirectToAction("Arsiv");
            }
        }

        // 9. VERİTABANINDAN KALICI SİLME
        [HttpGet("Book/KaliciSil/{id}")]
        public async Task<IActionResult> KaliciSil(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
                var mevcutKitap = response.Models.FirstOrDefault();
                if (mevcutKitap != null)
                {
                    await _supabaseClient.From<Book>().Delete(mevcutKitap);
                }
                return RedirectToAction("Arsiv");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kalıcı Silme Hatası: " + ex.Message);
                return RedirectToAction("Arsiv");
            }
        }
    }
}