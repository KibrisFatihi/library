using Postgrest.Attributes;
using Postgrest.Models;

namespace Kutuphane.Models
{
    [Table("readingbook")]
    public class Readingbook : BaseModel
    {
        [PrimaryKey("rbID", false)]
        public int Id { get; set; }

        [Column("bookID")]
        public int BookId { get; set; }

        [Column("mid")]
        public int MemberId { get; set; }

        [Column("takingDate")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Column("givingDate")]
        public DateTime? ReturnDate { get; set; }

        [Column("deliveryStatus")]
        public bool IsReturned { get; set; } = false;

        [Reference(typeof(Book))]
        public Book? Book { get; set; }

        [Reference(typeof(Member))]
        public Member? Member { get; set; }
    }
}