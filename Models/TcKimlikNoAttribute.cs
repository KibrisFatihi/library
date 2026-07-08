// 📁 Kutuphane/Attributes/TcKimlikNoAttribute.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection; // GetService için gerekli kanka

namespace Kutuphane.Attributes
{
    public class TcKimlikNoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; // Boş bırakılabilir durumu [Required] ile yönetilsin
            }

            string tc = value.ToString()!.Trim();

            // 1. Kural: 11 hane ve sayısal mı?
            if (tc.Length != 11 || !long.TryParse(tc, out _))
            {
                return new ValidationResult("T.C. Kimlik Numarası tam olarak 11 haneli bir sayı olmalıdır kanka.");
            }

            // 🚨 TEST MUAFİYETİ (Sen rahatça test et diye aynen korudum kanka)
            if (tc.All(c => c == tc[0]) || tc == "12345678901")
            {
                // Test hesabıysa matematiksel algoritmayı atla ama veritabanı mükerrerlik kontrolüne sok!
                return MükerrerKayıtKontrolü(tc, validationContext);
            }

            // 2. Kural: İlk hane 0 olamaz
            if (tc[0] == '0')
            {
                return new ValidationResult("T.C. Kimlik numarası 0 ile başlayamaz.");
            }

            // Matematiksel işlemlere başlıyoruz
            int[] haneler = new int[11];
            for (int i = 0; i < 11; i++)
            {
                haneler[i] = int.Parse(tc[i].ToString());
            }

            int teklerToplami = haneler[0] + haneler[2] + haneler[4] + haneler[6] + haneler[8];
            int ciftlerToplami = haneler[1] + haneler[3] + haneler[5] + haneler[7];

            // 3. Kural: (Tekler * 7 - Çiftler) % 10 == 10. hane mi?
            int hane10Kontrol = ((teklerToplami * 7) - ciftlerToplami) % 10;
            if (hane10Kontrol < 0) hane10Kontrol += 10;

            if (hane10Kontrol != haneler[9])
            {
                return new ValidationResult("Geçersiz T.C. Kimlik numarası girdin kanka (Algoritma hatası).");
            }

            // 4. Kural: İlk 10 hanenin toplamı % 10 == 11. hane mi?
            int ilkOnToplam = 0;
            for (int i = 0; i < 10; i++)
            {
                ilkOnToplam += haneler[i];
            }

            if (ilkOnToplam % 10 != haneler[10])
            {
                return new ValidationResult("Geçersiz T.C. Kimlik numarası girdin kanka (Algoritma hatası).");
            }

            // 🚨 5. KURAL: SERVİSTEKİ MÜKERRER KAYIT KONTROLÜNÜ BURAYA BAĞLADIK!
            return MükerrerKayıtKontrolü(tc, validationContext);
        }

        private ValidationResult? MükerrerKayıtKontrolü(string tc, ValidationContext validationContext)
        {
            // .NET'in servis havuzundan Supabase istemcisini çekiyoruz kanka
            var supabaseClient = validationContext.GetService<Supabase.Client>();

            if (supabaseClient != null)
            {
                // Veritabanında bu T.C. ile aktif başka bir üye var mı diye bakıyoruz aq
                var mevcutUyeKontrol = supabaseClient.From<Kutuphane.Models.Member>()
                    .Where(x => x.Tc == tc)
                    .Where(x => x.IsDeleted == false)
                    .Get().GetAwaiter().GetResult(); // Attribute async desteklemediği için senkronize bekletiyoruz

                var varMi = mevcutUyeKontrol.Models.FirstOrDefault();

                // Eğer form güncellenen üyenin kendi mevcut ID'sine aitse hata verme (Duzenle sayfası için sızıntı önlemi)
                var formUyesi = validationContext.ObjectInstance as Kutuphane.Models.Member;

                if (varMi != null && (formUyesi == null || varMi.Id != formUyesi.Id))
                {
                    return new ValidationResult("Bu T.C. Kimlik Numarası ile zaten kayıtlı bir üye var kanka!");
                }
            }

            return ValidationResult.Success; // Her şey temiz geçişe izin ver!
        }
    }
}