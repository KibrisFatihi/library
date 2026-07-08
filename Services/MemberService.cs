using Kutuphane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kutuphane.Services
{
    public class MemberService
    {
        private readonly Supabase.Client _supabaseClient;

        public MemberService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<Member>> GetActiveMembersAsync()
        {
            var response = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == false).Get();
            return response.Models;
        }

        public async Task AddMemberAsync(Member yeniUye)
        {
            yeniUye.IsDeleted = false;
            await _supabaseClient.From<Member>().Insert(yeniUye);
        }

        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            var response = await _supabaseClient.From<Member>().Where(x => x.Id == id).Get();
            return response.Models.FirstOrDefault();
        }

        public async Task UpdateMemberAsync(Member guncelUye)
        {
            guncelUye.IsDeleted = false;
            await _supabaseClient.From<Member>().Update(guncelUye);
        }

        public async Task SoftDeleteMemberAsync(int id)
        {
            var mevcut = await GetMemberByIdAsync(id);
            if (mevcut != null)
            {
                mevcut.IsDeleted = true;
                await _supabaseClient.From<Member>().Update(mevcut);
            }
        }

        public async Task<List<Member>> GetArchivedMembersAsync()
        {
            var response = await _supabaseClient.From<Member>().Where(x => x.IsDeleted == true).Get();
            return response.Models;
        }

        public async Task RecoverMemberFromArchiveAsync(int id)
        {
            var mevcut = await GetMemberByIdAsync(id);
            if (mevcut != null)
            {
                mevcut.IsDeleted = false;
                await _supabaseClient.From<Member>().Update(mevcut);
            }
        }

        public async Task HardDeleteMemberAsync(int id)
        {
            var mevcut = await GetMemberByIdAsync(id);
            try
            {
                if (mevcut != null)
                {
                    await _supabaseClient.From<Member>().Delete(mevcut);
                }
            }
            catch (Exception ex)
            {  
               Console.WriteLine($"Hata: {ex.Message}");
            }
        }
        public async Task<string?> RegisterNewMemberAsync(Member yeniUye)
        {
            // 1. Temel Boşluk Kontrolü
            if (string.IsNullOrEmpty(yeniUye?.Tc))
            {
                return "T.C. Kimlik Numarası boş bırakılamaz !";
            }

            // Sağdaki soldaki sinsi boşlukları temizliyoruz
            string temizTc = yeniUye.Tc.Trim();

            // 🚨 FORMAT KONTROLÜ 1: T.C. Kimlik Numarası KESİNLİKLE 11 hane olmalı!
            if (temizTc.Length != 11)
            {
                return "T.C. Kimlik Numarası tam olarak 11 haneli olmalıdır !";
            }

            // 🚨 FORMAT KONTROLÜ 2: T.C. Kimlik Numarası sadece rakamlardan oluşmalı!
            // (Araya harf veya çirkin karakter sızmasını engelliyoruz)
            bool sadeceSayiMi = temizTc.All(char.IsDigit);
            if (!sadeceSayiMi)
            {
                return "T.C. Kimlik Numarası sadece rakamlardan oluşabilir !";
            }

            // 2. Mükerrer Kayıt Kontrolü: Aynı TC veritabanında var mı?
            var mevcutUyeKontrol = await _supabaseClient.From<Member>()
                .Where(x => x.Tc == temizTc)
                .Where(x => x.IsDeleted == false)
                .Get();

            var varMi = mevcutUyeKontrol.Models.FirstOrDefault();
            if (varMi != null)
            {
                return "Bu T.C. Kimlik Numarası ile zaten kayıtlı bir üye var.!";
            } //Şifre kontrol modülleri
            if (string.IsNullOrEmpty(yeniUye.Password))
            {
                return "Şifre alanı boş bırakılamaz kanka!";
            }
            if (yeniUye.Password.Length < 6)
            {
                return "Şifre en az 6 karakter olmalıdır kanka!";
            }

            //  Kullanıcının yazdığı düz şifreyi (örn: 123456) alıp SHA256'ya çeviriyoruz
            yeniUye.Password = PasswordHasher.ComputeSha256Hash(yeniUye.Password.ToLower());
            // 3. Her şey formatına uygun ve benzersizse kaydet 
            yeniUye.Tc = temizTc;
            yeniUye.IsDeleted = false;
            yeniUye.Role = "Member";

            await _supabaseClient.From<Member>().Insert(yeniUye);

            return null; // Sıfır hata, tertemiz bitti 
        }
    }
}