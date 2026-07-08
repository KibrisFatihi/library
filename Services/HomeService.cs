// 📁 Services/HomeService.cs
using Kutuphane.Models;
using System;
using System.Threading.Tasks;
using Postgrest;


namespace kutuphane.Services
{
    public class HomeService : IHomeService
    {
        private readonly Supabase.Client _supabaseClient;

        public HomeService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<(int TotalBooks, int TotalMembers, int ActiveLoans)> GetDashboardStatsAsync()
        {
            try
            {
                // 1. Aktif Kitap Sayısı (IsDeleted == false olanlar)
                var bookResponse = await _supabaseClient.From<Book>()
                    .Where(x => x.IsDeleted == false)
                    .Count(Postgrest.Constants.CountType.Exact);
                int totalBooks = bookResponse;

                // 2. Aktif Üye Sayısı (IsDeleted == false olanlar)
                var memberResponse = await _supabaseClient.From<Member>()
                    .Where(x => x.IsDeleted == false)
                    .Count(Postgrest.Constants.CountType.Exact);
                int totalMembers = memberResponse;

                // 3. Teslim Edilmemiş Ödünç Kitap Sayısı (IsReturned == false olanlar)
                var loanResponse = await _supabaseClient.From<Readingbook>()
                    .Where(x => x.IsReturned == false)
                    .Count(Postgrest.Constants.CountType.Exact);
                int activeLoans = loanResponse;

                return (totalBooks, totalMembers, activeLoans);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dashboard istatistikleri çekilirken hata kanka: " + ex.Message);
                return (0, 0, 0); // Hata durumunda patlamasın, sıfır dönsün
            }
        }
    }
}