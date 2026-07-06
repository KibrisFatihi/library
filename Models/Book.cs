using Postgrest.Attributes;
using Postgrest.Models;

namespace Kutuphane.Models
{
    [Table("book")]
    public class Book : BaseModel
    {
        [PrimaryKey("bookid")]
        public int BookId { get; set; }

        [Column("bookname")]
        public string? BookName { get; set; }
        

        [Column("bookauthor")]
        public string? BookAuthor { get; set; }

        [Column("bookpage")]
        public int BookPage { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("locationID")]
        public int? LocationId { get; set; }
        [Reference(typeof(BookLocation))]
        public BookLocation? Location { get; set; }
        [Column("book_image_url")]
        public string? BookImageUrl { get; set; }
        [Column("stock_count")]
        public int? Stock { get; set; }
    }
}