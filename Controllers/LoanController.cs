using Kutuphane.Models;
using Microsoft.AspNetCore.Mvc;

namespace kutuphane.Controllers
{
    public class LoanController : Controller
    {
        private readonly Supabase.Client _supabaseClient;

        public LoanController(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        // 1. ÖDÜNÇ LİSTELEME SAYFASI
        public async Task<IActionResult> Index()
        {
            try
            {
                // Supabase'den teslim edilmemiş (aktif) ödünç alınan kitapları çekiyoruz
                var response = await _supabaseClient.From<Readingbook>()
                    .Where(x => x.IsReturned == false)
                    .Get();
                var oduncKitaplar = response.Models;

                return View(oduncKitaplar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ödünç alınan kitap listesi çekilirken hata oluştu: " + ex.Message);
                return View(new List<Readingbook>());
            }
        }

        // 2. KİTAP VERME EKRANI (GET)
        [HttpGet]
        public async Task<IActionResult> KitapVer()
        {
            try
            {
                // Açılır kutuları doldurmak için aktif kitapları ve üyeleri çekiyoruz
                var kitapResponse = await _supabaseClient.From<Book>().Where(x => x.IsDeleted == false).Get();
                var uyeResponse = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == false).Get();

                ViewBag.Kitaplar = kitapResponse.Models;
                ViewBag.Uyeler = uyeResponse.Models;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap verme sayfası açılırken hata: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 3. KİTAP VERME İŞLEMİNİ KAYDETME (POST)
        [HttpPost]
        public async Task<IActionResult> KitapVer(Readingbook yeniOdunc)
        {
            try
            {
                var uyeOduncResponse = await _supabaseClient.From<Readingbook>()
            .Where(x => x.MemberId == yeniOdunc.MemberId) // Bu üyenin kayıtlarına bak
            .Where(x => x.IsReturned == false)           // Henüz iade etmediklerini getir
            .Get();

                var aktifOduncSayisi = uyeOduncResponse.Models.Count;

                if (aktifOduncSayisi >= 2)
                {
                    ViewBag.HataMesaji = "Bu üye maksimum kitap sınırına (2 adet) ulaşmıştır. Yeni kitap alabilmesi için önce elindekini iade etmelidir !";
                    await YenidenModeliDoldur();
                    return View(yeniOdunc);
                }

                var ayniKitapVarMi = uyeOduncResponse.Models.Any(x => x.BookId == yeniOdunc.BookId);

                if (ayniKitapVarMi)
                {
                    ViewBag.HataMesaji = "Bu üye bu kitabı zaten ödünç almış ve henüz iade etmemiş ! Aynı kitap tekrar verilemez.";
                    await YenidenModeliDoldur();
                    return View(yeniOdunc);
                }
                // 1. Önce asıl "Book" tablosundan kitabı çekiyoruz kanka
                var bookResponse = await _supabaseClient.From<Book>()
                    .Where(x => x.BookId == yeniOdunc.BookId)
                    .Get();

                var secilenKitap = bookResponse.Models.FirstOrDefault();

                // DOĞRU KULLANIM: Stok kontrolünü 'yeniOdunc' üzerinden değil, 
                // 'secilenKitap' (yani Book modeli) üzerinden yapıyoruz!
                if (secilenKitap == null || secilenKitap.Stock <= 0)
                {

                    ViewBag.HataMesaji = "Seçilen kitap stokta kalmadığı için ödünç verilemez !";
                    var kitapResponse = await _supabaseClient.From<Book>().Where(x => x.IsDeleted == false).Get();
                    var uyeResponse = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == false).Get();
                    ViewBag.Kitaplar = kitapResponse.Models;
                    ViewBag.Uyeler = uyeResponse.Models;

                    // 3. KRİTİK DEĞİŞİKLİK: Redirect etmiyoruz, direkt mevcut View'u ekrana basıyoruz!
                    return View(yeniOdunc);
                }

                // Stok azaltmayı da yine Book nesnesi üzerinde yapıyoruz
                secilenKitap.Stock--;
                await _supabaseClient.From<Book>().Update(secilenKitap);

                // Ödünç kaydını (Readingbook) ise olduğu gibi kaydediyoruz kanka
                yeniOdunc.IsReturned = false;
                await _supabaseClient.From<Readingbook>().Insert(yeniOdunc);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap verilirken hata oluştu: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // 4. KİTABI İADE ALMA
        [HttpGet]
        public async Task<IActionResult> ReturnBook(int id)
        {
            try
            {
                var response = await _supabaseClient.From<Readingbook>()
                    .Where(x => x.Id == id)
                    .Get();

                var oduncKitap = response.Models.FirstOrDefault();

                if (oduncKitap != null)
                {
                    // 1. Kitabı iade edildi yapıyoruz
                    oduncKitap.IsReturned = true;
                    oduncKitap.ReturnDate = DateTime.Now;
                    await _supabaseClient.From<Readingbook>().Update(oduncKitap);

                    // 2. KANKA BURAYI EKLE: Kitabın stokunu geri 1 artırıyoruz!
                    var bookResponse = await _supabaseClient.From<Book>()
                        .Where(x => x.BookId == oduncKitap.BookId)
                        .Get();

                    var iadeEdilenKitap = bookResponse.Models.FirstOrDefault();
                    if (iadeEdilenKitap != null)
                    {
                        iadeEdilenKitap.Stock++; // Kütüphaneye geri döndü, stoku artırdık
                        await _supabaseClient.From<Book>().Update(iadeEdilenKitap);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kitap iade alınırken hata oluştu: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
        async Task YenidenModeliDoldur()
        {
            var kitapResponse = await _supabaseClient.From<Book>().Where(x => x.IsDeleted == false).Get();
            var uyeResponse = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == false).Get();
            ViewBag.Kitaplar = kitapResponse.Models;
            ViewBag.Uyeler = uyeResponse.Models;
        }
    }
}