using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLMS.Api.Models
{
    public enum UserRole { User, Admin }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required, MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public bool EmailConfirmed { get; set; }
        public string? EmailConfirmationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}