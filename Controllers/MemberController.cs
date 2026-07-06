using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;


namespace kutuphane.Controllers
{

    public class MemberController : Controller
    {
        private readonly Supabase.Client _supabaseClient;

        public MemberController(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Supabase'den silinmemiş (aktif) üye çekiyoruz
                var response = await _supabaseClient.From<Member>()
                    .Where(x => x.IsDeleted == false)
                    .Get();

                var member = response.Models;
               

                // Views/Member/Index.cshtml sayfasına verileri gönderiyoruz kanka
                return View(member);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Üye listesi çekilirken hata oluştu: " + ex.Message);
                return View(new List<Member>());
            }
        }
        // 1. Yeni üye ekleme sayfasını açan fonksiyon (GET)
        [HttpGet]
        public IActionResult Ekle()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ekle sayfasında hata oluştu: " + ex.Message);
                return View();
            }
        }

        // 2. Formdan gelen yeni üye kaydeden fonksiyon (POST)
        [HttpPost]
        public async Task<IActionResult> Ekle(Member yeniUye)
        {
            if (!ModelState.IsValid)
            {
                return View(yeniUye); // Girilen verileri kaybetmeden sayfaya geri gönderir
            }
            try
            {
                yeniUye.IsDeleted = false; // Yeni üye silinmemiş olarak başlar
                await _supabaseClient.From<Member>().Insert(yeniUye);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap eklenirken hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 3. Yazılımsal Silme (Soft Delete) Fonksiyonu
        [HttpGet("Member/Sil/{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Member>().Where(x => x.Id == id).Get();
                var mevcutUye = response.Models.FirstOrDefault();

                if (mevcutUye != null)
                {
                    mevcutUye.IsDeleted = true;
                    await _supabaseClient.From<Member>().Update(mevcutUye);
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
        [HttpGet("Member/Duzenle/{id}")]
        public async Task<IActionResult> Duzenle(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Member>().Where(x => x.Id == id).Get();
                var mevcutUye = response.Models.FirstOrDefault();
                if (mevcutUye != null)
                {
                    return View(mevcutUye);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Düzenleme sayfası açılırken hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }


        // 5. Formdan gelen güncel verileri Supabase'e kaydeden fonksiyon (POST)
        [HttpPost]
        public async Task<IActionResult> Duzenle(Member guncelKitap)
        {
            try
            {
                guncelKitap.IsDeleted = false;
                await _supabaseClient.From<Member>().Update(guncelKitap);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap Güncellenirken Hata Oluştu: " + ex.Message);
                return View(guncelKitap);
            }
        }
        [HttpGet("Member/Arsiv")]
        public async Task<IActionResult> Arsiv()
        {
            try
            {
                var response = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == true).Get();
                var silinmisUyeler = response.Models;
                return View(silinmisUyeler);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Arşiv sayfası açılırken hata: " + ex.Message);
                return View(new List<Member>());
            }
        }
        [HttpGet("Member/GeriAl/{id}")]
        public async Task<IActionResult> GeriAl(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Member>().Where(x => x.Id == id).Get();
                var mevcutUye = response.Models.FirstOrDefault();
                if (mevcutUye != null)
                {
                    mevcutUye.IsDeleted = false;
                    await _supabaseClient.From<Member>().Update(mevcutUye);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Soft Delete Geri Alma Hatası: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
        [HttpGet("Member/KalıcıSil")]
        public async Task<IActionResult> KalıcıSil(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Member>().Where(x => x.Id == id).Get();
                var mevcutuye = response.Models.FirstOrDefault();
                if (mevcutuye != null)
                {
                    await _supabaseClient.From<Member>().Delete(mevcutuye);
                }
                return RedirectToAction("Arsiv");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kalıcı Silme Hatası: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

    }
}
