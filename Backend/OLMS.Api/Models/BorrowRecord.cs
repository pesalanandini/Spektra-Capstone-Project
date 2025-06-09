using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLMS.Api.Models
{
    public class BorrowRecord
    {
        [Key]
        public int BorrowRecordId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public required User User { get; set; } // Added 'required' modifier

        [ForeignKey(nameof(Book))]
        public int BookId { get; set; }

        public Book Book { get; set; } = null!; // Use null-forgiving operator

        public DateTime BorrowedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsReturned { get; set; }
    }
}