using Postgrest.Attributes;
using Postgrest.Models;

namespace Kutuphane.Models
{
    [Table("location")]
    public class BookLocation : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("raf")]
        public string? Raf { get; set; }

        [Column("sira")]
        public string? Sira { get; set; }
    }
}