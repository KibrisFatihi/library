// 📁 Services/ILoanService.cs
using Kutuphane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kutuphane.Services
{
    public interface ILoanService
    {
        // 1. Aktif ödünçleri getiren metot imzası
        Task<List<Readingbook>> GetActiveLoansAsync();

        // 2. Kitap ve Üye listelerini tuple olarak dönen form metot imzası
        Task<(List<Book> Kitaplar, List<Member> Uyeler)> GetLoanFormDataAsync();

        // 3. Kitap ödünç verme ve doğrulama (validation) yapan metot imzası
        Task<string?> BorrowBookAsync(Readingbook yeniOdunc);

        // 4. Kitap iade alma metot imzası
        Task ReturnBookAsync(int id);
    }
}