using Kutuphane.Models;
using Postgrest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kutuphane.Services
{
    public class BookService
    {
        private readonly Supabase.Client _supabaseClient;

        public BookService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<Book>> GetActiveBooksAsync()
        {
            var response = await _supabaseClient.From<Book>()
                .Where(x => x.IsDeleted == false)
                .Order(x => x.BookId, Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;
        }

        public async Task<List<Book>> SearchBooksAsync(string arananKelime)
        {
            var rpcResponse = await _supabaseClient.Rpc<List<Book>>("ara_kitaplar", new Dictionary<string, object>
            {
                { "aranan", arananKelime.Trim() }
            });
            return rpcResponse;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
            return response.Models.FirstOrDefault();
        }

        public async Task AddBookAsync(Book yeniKitap)
        {
            yeniKitap.IsDeleted = false;
            await _supabaseClient.From<Book>().Insert(yeniKitap);
        }

        public async Task UpdateBookAsync(Book guncelKitap)
        {
            guncelKitap.IsDeleted = false;
            await _supabaseClient.From<Book>().Update(guncelKitap);
        }

        public async Task SoftDeleteBookAsync(int id)
        {
            var mevcut = await GetBookByIdAsync(id);
            if (mevcut != null)
            {
                mevcut.IsDeleted = true;
                await _supabaseClient.From<Book>().Update(mevcut);
            }
        }

        public async Task<List<Book>> GetArchivedBooksAsync()
        {
            var response = await _supabaseClient.From<Book>().Where(x => x.IsDeleted == true).Get();
            return response.Models;
        }

        public async Task RecoverBookFromArchiveAsync(int id)
        {
            var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
            var mevcut = response.Models.FirstOrDefault();
            if (mevcut != null)
            {
                mevcut.IsDeleted = false;
                await _supabaseClient.From<Book>().Update(mevcut);
            }
        }

        public async Task HardDeleteBookAsync(int id)
        {
            var response = await _supabaseClient.From<Book>().Where(x => x.BookId == id).Get();
            var mevcut = response.Models.FirstOrDefault();
            if (mevcut != null)
            {
                await _supabaseClient.From<Book>().Delete(mevcut);
            }
        }
    }
}