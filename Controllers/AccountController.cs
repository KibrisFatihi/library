using Kutuphane.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using kutuphane.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace kutuphane.Controllers
{
    public class AccountController : Controller
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly MemberService _memberService;

        public AccountController(Supabase.Client supabaseClient, MemberService memberService)
        {
            _supabaseClient = supabaseClient;
            _memberService = memberService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // 🚨 DÖNGÜ KIRICI: Eğer kullanıcı zaten doğrulanmışsa döngüye sokmadan direkt uçur kanka
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "Admin") return RedirectToAction("Index", "Home");
                return RedirectToAction("Index", "MemberDashboard");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Şifre boş bırakılamaz kanka!";
                    return View();
                }

                string hashlenmisSifre = PasswordHasher.ComputeSha256Hash(password);

                var response = await _supabaseClient.From<Member>()
                    .Where(x => x.Mail == email)
                    .Where(x => x.IsDeleted == false)
                    .Get();

                var user = response.Models.FirstOrDefault();

                // 🚨 ÇİFT YÖNLÜ ŞİFRE KONTROLÜ VE PARANTEZ TAMİRİ
                if (user != null && (user.Password == password || user.Password == hashlenmisSifre))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim("sub", user.Id.ToString()),
                        new Claim(ClaimTypes.Name, $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim()),
                        new Claim(ClaimTypes.Email, user.Mail ?? ""),
                        new Claim(ClaimTypes.Role, user.Role ?? "Member")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    // Çerezi tarayıcı belleğine kazıyoruz kanka
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Yönlendirme mantığı
                    if (user.Role?.Trim() == "Admin")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "MemberDashboard");
                    }
                }

                // Kullanıcı bulunamadıysa veya şifreler uyuşmadıysa buraya düşer kanka
                ViewBag.Error = "E-posta veya şifre hatalı !";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş yapılırken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Member yeniUye)
        {
            try
            {
                // Bütün format ve mükerrerlik kontrollerini servise paslıyoruz.
                var hataMesaji = await _memberService.RegisterNewMemberAsync(yeniUye);

                if (hataMesaji != null)
                {
                    ViewBag.Error = hataMesaji;
                    return View(yeniUye);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Kayıt olurken beklenmedik bir hata oluştu kanka: " + ex.Message;
                return View(yeniUye);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Welcome()
        {
            ViewBag.ToplamKitap = "342,850+";
            ViewBag.AktifUye = "12,450+";
            ViewBag.GunlukZiyaret = "1,850+";
            ViewBag.DolulukOrani = "78";
            return View();
        }
    }
}