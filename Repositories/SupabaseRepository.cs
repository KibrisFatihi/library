// 📁 Repositories/SupabaseRepository.cs
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq; // FirstOrDefault için eklendi
using System.Threading.Tasks;
using Postgrest.Models;

namespace kutuphane.Repositories
{
    public class SupabaseRepository<T> : IRepository<T> where T : BaseModel, new()
    {
        private readonly Client _supabaseClient;

        public SupabaseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var response = await _supabaseClient.From<T>().Get();
            // Eğer veritabanı boşsa null yerine boş liste dönerek koruma sağlıyoruz 
            return response.Models ?? new List<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var response = await _supabaseClient.From<T>().Get();

            if (response.Models == null) return null;

            var entity = response.Models.FirstOrDefault(x =>
            {
                var property = x.GetType().GetProperties()
                    .FirstOrDefault(p => p.Name.ToLower().Contains("id") || p.Name.ToLower().EndsWith("id"));

                if (property == null) return false;

                var value = property.GetValue(x);
                return value != null && Convert.ToInt32(value) == id;
            });

            return entity;
        }

        public async Task<bool> AddAsync(T entity)
        {
            var response = await _supabaseClient.From<T>().Insert(entity);
            return response.ResponseMessage?.IsSuccessStatusCode ?? false;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var response = await _supabaseClient.From<T>().Update(entity);
            return response.ResponseMessage?.IsSuccessStatusCode ?? false;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            var response = await _supabaseClient.From<T>().Delete(entity);
            return response.ResponseMessage?.IsSuccessStatusCode ?? false;
        }
    }
}