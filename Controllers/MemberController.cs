using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;
using kutuphane.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace kutuphane.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MemberController : Controller
    {
        private readonly MemberService _memberService;

        public MemberController(MemberService memberService)
        {
            _memberService = memberService;
        }

        public async Task<IActionResult> Index()
        {
            var üyeler = await _memberService.GetActiveMembersAsync();
            return View(üyeler);
        }

        [HttpGet]
        public IActionResult Ekle()
        {
            ViewBag.Roller = new List<string> { "Member", "Admin" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Member yeniUye)
        {
            try
            {
                // 1. Adım: [TcKimlikNo] model doğrulaması geçerli mi kontrol et
                if (!ModelState.IsValid)
                {
                    var ilkHata = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    ViewBag.Error = ilkHata ?? "Bilgileri kontrol ediniz.";
                    ViewBag.Roller = new List<string> { "Member", "Admin" };
                    return View(yeniUye);
                }

                // 2. Adım: Servisteki hazır şifre ve T.C. kontrol iş motorunu çalıştır
                // Bu metot şifre uzunluğunu kontrol eder, hash'ler ve veritabanına doğrudan kaydeder.
                var hataMesaji = await _memberService.RegisterNewMemberAsync(yeniUye);

                // 3. Adım: Eğer servisten şifre uzunluğu veya mükerrerlikle ilgili bir engel döndüyse yakala
                if (hataMesaji != null)
                {
                    ViewBag.Error = hataMesaji;
                    ViewBag.Roller = new List<string> { "Member", "Admin" };
                    return View(yeniUye);
                }

                // İşlem tamamen başarılıysa ana listeye yönlendir
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Üye eklenirken bir hata oluştu: " + ex.Message;
                ViewBag.Roller = new List<string> { "Member", "Admin" };
                return View(yeniUye);
            }
        }




        [HttpGet("Member/Sil/{id}")]
        public async Task<IActionResult> Sil(int id)
        {
            await _memberService.SoftDeleteMemberAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet("Member/Duzenle/{id}")]
        public async Task<IActionResult> Duzenle(int id)
        {
            ViewBag.Roller = new List<string> { "Member", "Admin" };

            var uye = await _memberService.GetMemberByIdAsync(id);
            if (uye == null) return RedirectToAction("Index");
            return View(uye);
        }

        [HttpPost]
        public async Task<IActionResult> Duzenle(Member guncelUye)
        {
            try
            {
                // 🚨 EN KRİTİK ADMİN DÜZENLEME ÖNLEMİ:
                // Admin bir üyeyi düzenlerken şifresini değiştirdiyse veya değiştirmeden kaydettiyse durumuna bakıyoruz.
                if (!string.IsNullOrEmpty(guncelUye.Password))
                {
                    // Eğer şifre alanı dolu geldiyse, girilen yeni şifreyi SHA256 ile hash'leyip öyle kaydediyoruz kanka!
                    // Eğer şifre zaten hash'liyse (veri tabanından gelen veri bozulmadan post edildiyse uzunluğu 64 karakterdir) tekrar hash'lemiyoruz.
                    if (guncelUye.Password.Length != 64)
                    {
                        guncelUye.Password = PasswordHasher.ComputeSha256Hash(guncelUye.Password);
                    }
                }

                await _memberService.UpdateMemberAsync(guncelUye);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Güncelleme yapılırken hata oluştu: " + ex.Message;
                ViewBag.Roller = new List<string> { "Member", "Admin" };
                return View(guncelUye);
            }
        }

        [HttpGet("Member/Arsiv")]
        public async Task<IActionResult> Arsiv()
        {
            var arsiv = await _memberService.GetArchivedMembersAsync();
            return View(arsiv);
        }

        [HttpGet("Member/GeriAl/{id}")]
        public async Task<IActionResult> GeriAl(int id)
        {
            await _memberService.RecoverMemberFromArchiveAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet("Member/KaliciSil/{id}")]
        public async Task<IActionResult> KaliciSil(int id)
        {
            await _memberService.HardDeleteMemberAsync(id);
            return RedirectToAction("Arsiv");
        }
        [HttpGet("Member/SifreSifirla/{id}")]
        public async Task<IActionResult> SifreSifirla(int id)
        {
            try
            {
                var uye = await _memberService.GetMemberByIdAsync(id);

                if (uye != null)
                {
                    // 1. 6 haneli rastgele şifreyi ham olarak üretiyoruz kanka
                    string hamSifre = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

                    // 🚨 SİBER SABİTLEME: Hash motoruna göndermeden önce şifreyi tamamen KÜÇÜK harfe çekiyoruz!
                    // Böylece Login sayfasında kullanıcı ne yazarsa yazsın hash'ler tam olarak eşleşecek .
                    uye.Password = PasswordHasher.ComputeSha256Hash(hamSifre);

                    // Veritabanını güncelliyoruz
                    await _memberService.UpdateMemberAsync(uye);

                    
                    // Adminin ve üyenin kafa karışıklığı yaşamaması için ekranda BÜYÜK harfle gösterelim,
                  
                    string ekrandaGorunecekSifre = hamSifre.ToUpper();

                    TempData["SuccessMessage"] = $"{uye.FirstName} {uye.LastName} için YENİ ŞİFRE: '{ekrandaGorunecekSifre}' olarak üretildi. Lütfen bunu üyeye ilet !";
                }
                else
                {
                    TempData["ErrorMessage"] = "Üye bulunamadı !";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Şifre sıfırlanırken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Duzenle", new { id = id });
        }

    }
    
}