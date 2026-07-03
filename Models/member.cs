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
        [Column("mEMail")]
        public string? Mail { get; set; }


    }
}