using System.ComponentModel.DataAnnotations;

namespace Kutuphane.Attributes
{
    public class TcKimlikNoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; // Boş bırakılabilir kuralı [Required] ile yönetilsin diye
            }

            string tc = value.ToString()!;

            // 1. Kural: 11 hane ve sayısal mı?
            if (tc.Length != 11 || !long.TryParse(tc, out _))
            {
                return new ValidationResult("TC Kimlik numarası tam 11 haneli bir sayı olmalıdır kanka.");
            }

            // 2. Kural: İlk hane 0 olamaz
            if (tc[0] == '0')
            {
                return new ValidationResult("TC Kimlik numarası 0 ile başlayamaz.");
            }

            // Dizilere döküp matematiksel işlemlere başlayalım
            int[] haneler = new int[11];
            for (int i = 0; i < 11; i++)
            {
                haneler[i] = int.Parse(tc[i].ToString());
            }

            int teklerToplami = haneler[0] + haneler[2] + haneler[4] + haneler[6] + haneler[8];
            int ciftlerToplami = haneler[1] + haneler[3] + haneler[5] + haneler[7];

            // 3. Kural: (Tekler * 7 - Çiftler) % 10 == 10. hane mi?
            int hane10Kontrol = ((teklerToplami * 7) - ciftlerToplami) % 10;
            if (hane10Kontrol < 0) hane10Kontrol += 10; // Negatif mod kontrolü

            if (hane10Kontrol != haneler[9])
            {
                return new ValidationResult("Geçersiz TC Kimlik numarası girdin kanka (Algoritma hatası).");
            }

            // 4. Kural: İlk 10 hanenin toplamı % 10 == 11. hane mi?
            int ilkOnToplam = 0;
            for (int i = 0; i < 10; i++)
            {
                ilkOnToplam += haneler[i];
            }

            if (ilkOnToplam % 10 != haneler[10])
            {
                return new ValidationResult("Geçersiz TC Kimlik numarası girdin kanka (Algoritma hatası).");
            }

            return ValidationResult.Success; // Her şey tamamsa geçişe izin ver!
        }
    }
}