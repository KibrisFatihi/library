using Kutuphane.Attributes;
using Postgrest.Attributes;
using Postgrest.Models;
using System.ComponentModel.DataAnnotations; // Validation için ŞART 

namespace Kutuphane.Models
{
    [Table("member")]
    public class Member : BaseModel
    {
        [PrimaryKey("mID", false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "TC Kimlik numarası boş bırakılamaz kanka.")]
        [TcKimlikNo]
        [Column("mTC")]
        public string? Tc { get; set; }

        [Required(ErrorMessage = "Üye adı boş bırakılamaz.")]
        [Column("mFName")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Üye soyadı boş bırakılamaz.")]
        [Column("mLName")]
        public string? LastName { get; set; }

        // TELEFON NUMARASI DOĞRULAMA (05xx xxx xx xx formatı için Regex)
        [Required(ErrorMessage = "Telefon numarası boş bırakılamaz.")]
        [RegularExpression(@"^(05\d{9})$", ErrorMessage = "Telefon numarası 05 ile başlamalı ve başında boşluk olmadan toplam 11 hane olmalıdır (Örn: 05321234567).")]
        [Column("mTel")]
        public string? Telephone { get; set; }

        // E-POSTA FORMATI DOĞRULAMA
        [Required(ErrorMessage = "E-posta adresi boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi gir (Örn: isim@domain.com).")]
        [Column("email")]
        public string? Mail { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

    
    [Column("password")]
        public string? Password { get; set; }

        [Column("role")]
        public string Role { get; set; } = "Member";
} }