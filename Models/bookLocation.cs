using Postgrest.Attributes;
using Postgrest.Models;

namespace Kutuphane.Models
{
    [Table("booklocation")]
    public class BookLocation : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("raf")]
        public string? raf { get; set; }

        [Column("sira")]
        public string? sira { get; set; }
    }
}