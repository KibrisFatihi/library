// 📁 Services/BookLocationService.cs
using Kutuphane.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kutuphane.Services
{
    public class BookLocationService
    {
        private readonly Client _supabaseClient;

        // Modüler yapı hata verdiği için constructor ile client alıyoruz
        public BookLocationService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<BookLocation>> GetAllLocationsAsync()
        {
            try
            {
                
                var response = await _supabaseClient.From<BookLocation>().Get();
                return response.Models ?? new List<BookLocation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Servis içinde konumlar çekilemedi: " + ex.Message);
                return new List<BookLocation>();
            }
        }
    }
}