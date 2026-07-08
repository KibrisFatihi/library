    using Kutuphane.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

namespace kutuphane.Services
{
    public class LoanService : ILoanService
    {
        private readonly Supabase.Client _supabaseClient;

        public LoanService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<Readingbook>> GetActiveLoansAsync()
        {
            var response = await _supabaseClient.From<Readingbook>()
                .Where(x => x.IsReturned == false)
                .Get();
            return response.Models;
        }

        public async Task<(List<Book> Kitaplar, List<Member> Uyeler)> GetLoanFormDataAsync()
        {
            var kitapResponse = await _supabaseClient.From<Book>().Where(x => x.IsDeleted == false && x.Stock > 0).Get();
            var uyeResponse = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == false).Get();
            return (kitapResponse.Models, uyeResponse.Models);
        }

        public async Task<string?> BorrowBookAsync(Readingbook yeniOdunc)
        {
            var uyeOduncResponse = await _supabaseClient.From<Readingbook>()
                .Where(x => x.MemberId == yeniOdunc.MemberId)
                .Where(x => x.IsReturned == false)
                .Get();

            if (uyeOduncResponse.Models.Count >= 2)
            {
                return "Bu üye maksimum kitap sınırına (2 adet) ulaşmıştır. Yeni kitap alabilmesi için önce elindekini iade etmelidir !";
            }

            if (uyeOduncResponse.Models.Any(x => x.BookId == yeniOdunc.BookId))
            {
                return "Bu üye bu kitabı zaten ödünç almış ve henüz iade etmemiş ! Aynı kitap tekrar verilemez.";
            }

            var bookResponse = await _supabaseClient.From<Book>().Where(x => x.BookId == yeniOdunc.BookId).Get();
            var secilenKitap = bookResponse.Models.FirstOrDefault();

            if (secilenKitap == null || secilenKitap.Stock <= 0)
            {
                return "Seçilen kitap stokta kalmadığı için ödünç verilemez !";
            }

            // Stok düşme işlemi serviste güvenli kanka
            secilenKitap.Stock--;
            await _supabaseClient.From<Book>().Update(secilenKitap);

            yeniOdunc.IsReturned = false;
            yeniOdunc.ReturnDate = DateTime.Now.AddDays(14);
            await _supabaseClient.From<Readingbook>().Insert(yeniOdunc);

            return null; // Hata yoksa null döner
        }

        public async Task ReturnBookAsync(int id)
        {
            var response = await _supabaseClient.From<Readingbook>().Where(x => x.Id == id).Get();
            var oduncKitap = response.Models.FirstOrDefault();

            if (oduncKitap != null)
            {
                oduncKitap.IsReturned = true;
                oduncKitap.ReturnDate = DateTime.Now;
                await _supabaseClient.From<Readingbook>().Update(oduncKitap);

                var bookResponse = await _supabaseClient.From<Book>().Where(x => x.BookId == oduncKitap.BookId).Get();
                var iadeEdilenKitap = bookResponse.Models.FirstOrDefault();
                if (iadeEdilenKitap != null)
                {
                    iadeEdilenKitap.Stock++;
                    await _supabaseClient.From<Book>().Update(iadeEdilenKitap);
                }
            }
        }
        public async Task<List<Readingbook>> GetMemberActiveLoansAsync(int memberId)
        {
            var response = await _supabaseClient.From<Readingbook>()
                .Where(x => x.MemberId == memberId)
                .Where(x => x.IsReturned == false)
                .Get();
            return response.Models;
        }
    }
}