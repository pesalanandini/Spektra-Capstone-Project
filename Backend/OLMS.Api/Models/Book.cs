using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLMS.Api.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required, MaxLength(200)]
        public string? Title { get; set; }

        [Required, MaxLength(100)]
        public string? Author { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty; // Default value added

        [MaxLength(100)]
        public string Genre { get; set; } = string.Empty; // Default value added

        public string CoverImageUrl { get; set; } = string.Empty; // Default value added
        public string PdfUrl { get; set; } = string.Empty; // Default value added
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
    }
}