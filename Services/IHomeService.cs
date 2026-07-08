// 📁 Services/IHomeService.cs
using System.Threading.Tasks;

namespace kutuphane.Services
{
    public interface IHomeService
    {
        Task<(int TotalBooks, int TotalMembers, int ActiveLoans)> GetDashboardStatsAsync();
    }
}