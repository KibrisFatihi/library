
using System;
using System.Security.Cryptography;
using System.Text;

namespace kutuphane.Services
{
    public static class PasswordHasher
    {
       
        public static string ComputeSha256Hash(string rawData)
        {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Şifreyi byte dizisine çevirip hash'liyoruz
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Byte dizisini string (Hex) formatına dönüştürüyoruz
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}