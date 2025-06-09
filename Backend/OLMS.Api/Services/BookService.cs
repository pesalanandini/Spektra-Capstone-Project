using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OLMS.Api.Data;
using OLMS.Api.Models;

namespace OLMS.Api.Services
{
    public class BookService : IBookService
    {
        private readonly OLMSDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public BookService(OLMSDbContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }
        public async Task<IEnumerable<Book>> GetAllBooksAsync(string? search = null, string? genre = null, string? findAll = null)
        {
            var query = _context.Books.AsQueryable();

            // If findAll flag is explicitly sent as "allBooks", skip filters and return everything
            if (!string.IsNullOrWhiteSpace(findAll) && findAll.ToLower() == "allbooks")
            {
                return await query.ToListAsync();
            }

            // Apply genre filter if specified
            if (!string.IsNullOrWhiteSpace(genre))
            {
                string lowerGenre = genre.ToLower();
                query = query.Where(b => b.Genre != null && b.Genre.ToLower().Contains(lowerGenre));
            }

            // Apply search keyword filter across multiple fields
            if (!string.IsNullOrWhiteSpace(search))
            {
                string lowerSearch = search.ToLower();
                query = query.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(lowerSearch)) ||
                    (b.Author != null && b.Author.ToLower().Contains(lowerSearch)) ||
                    (b.Genre != null && b.Genre.ToLower().Contains(lowerSearch)) ||
                    (b.Description != null && b.Description.ToLower().Contains(lowerSearch)) ||
                    (b.PdfUrl != null && b.PdfUrl.ToLower().Contains(lowerSearch)) ||
                    (b.CoverImageUrl != null && b.CoverImageUrl.ToLower().Contains(lowerSearch))
                );
            }

            return await query.ToListAsync();
         }
        public async Task<Book> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) throw new Exception("Book not found.");
            return book;
        }

        public async Task AddBookAsync(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            var existing = await _context.Books.FindAsync(book.BookId);
            if (existing == null) throw new Exception("Book not found.");
            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.Description = book.Description;
            existing.Genre = book.Genre;
            existing.CoverImageUrl = book.CoverImageUrl;
            existing.PdfUrl = book.PdfUrl;
            existing.AvailableCopies = book.AvailableCopies;
            existing.TotalCopies = book.TotalCopies;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) throw new Exception("Book not found.");
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task BorrowBookAsync(int userId, int bookId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@BookId", bookId)
            };
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_BorrowBook @UserId, @BookId", parameters);
        }

        public async Task ReturnBookAsync(int userId, int bookId)
        {
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@BookId", bookId),
                new SqlParameter("@ReturnDate", DateTime.UtcNow)
            };
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_ReturnBook @UserId, @BookId, @ReturnDate", parameters);
        }

        public async Task<ReportResult> GenerateReportAsync()
        {
            var result = await _context.Set<ReportResult>().FromSqlRaw("EXEC sp_GenerateReport").ToListAsync();
            return result.FirstOrDefault() ?? new ReportResult();
        }


        public async Task CalculateFineAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_CalculateFine");
        }
        public async Task<IEnumerable<BorrowRecord>> GetAllBorrowRecordsAsync()
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .ToListAsync();
        }
        public async Task<IEnumerable<Fine>> GetAllFinesAsync()
        {
            return await _context.Fines
                .Include(f => f.BorrowRecord)
                    .ThenInclude(br => br.User)
                .Include(f => f.BorrowRecord)
                    .ThenInclude(br => br.Book)
                .ToListAsync();
        }

    }
}
