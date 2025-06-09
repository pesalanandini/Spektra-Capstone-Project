using System.ComponentModel.DataAnnotations;

namespace OLMS.Api.Models.DTOs
{
    public class BookUpdateDto
    {
        [Required, MaxLength(200)]
        public string? Title { get; set; }

        [Required, MaxLength(100)]
        public string? Author { get; set; }

        [Required, MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        public string? CoverImageUrl { get; set; }
        public string? PdfUrl { get; set; }
        [Required]
        public int TotalCopies { get; set; }
    }
}