using Microsoft.EntityFrameworkCore;
using OLMS.Api.Models;

namespace OLMS.Api.Data
{
    public class OLMSDbContext : DbContext
    {
        public OLMSDbContext(DbContextOptions<OLMSDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<ReportResult> ReportResults { get; set; } // For SQL/raw reporting

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint on email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Book default values
            modelBuilder.Entity<Book>()
                .Property(b => b.AvailableCopies)
                .HasDefaultValue(1);

            modelBuilder.Entity<Book>()
                .Property(b => b.TotalCopies)
                .HasDefaultValue(1);

            // BorrowRecord relationships
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany()
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of users

            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany()
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of books

            // Fine relationship
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.BorrowRecord)
                .WithMany()
                .HasForeignKey(f => f.BorrowRecordId)
                .OnDelete(DeleteBehavior.Cascade); // If a borrow record is deleted, fine goes too

            // Fine decimal precision (important for money values)
            modelBuilder.Entity<Fine>()
                .Property(f => f.FineAmount)
                .HasPrecision(10, 2);

            // ReportResult - keyless view/model for reports
            modelBuilder.Entity<ReportResult>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // Not mapped to table or view
            });
        }
    }
}
