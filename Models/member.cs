using Postgrest.Attributes;
using Postgrest.Models;

namespace Kutuphane.Models
{
    [Table("member")]
    public class Member : BaseModel
    {
        [PrimaryKey("mID, false")]
        public int Id { get; set; }

        [Column("mTC")]
        public string? Tc { get; set; }

        [Column("mFName")]
        public string? FirstName { get; set; }
        [Column("mLName")]
        public string? LastName { get; set; }
        [Column("mTel")]
        public string? Telephone { get; set; }

        // E-POSTA FORMATI DOĞRULAMA
        [Required(ErrorMessage = "E-posta adresi boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi gir kanka (Örn: isim@domain.com).")]
        [Column("email")]
        public string? Mail { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}