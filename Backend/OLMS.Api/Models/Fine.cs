using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLMS.Api.Models
{
    public class Fine
    {
        [Key]
        public int FineId { get; set; }

        [ForeignKey(nameof(BorrowRecord))]
        public int BorrowRecordId { get; set; }
        public BorrowRecord? BorrowRecord { get; set; }

        public decimal FineAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}