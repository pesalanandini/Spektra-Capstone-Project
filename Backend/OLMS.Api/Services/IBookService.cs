using System.Collections.Generic;
using System.Threading.Tasks;
using OLMS.Api.Models;

namespace OLMS.Api.Services
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(string? search = null, string? genre = null, string? findAll = null);
        Task<Book> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int bookId);
        Task BorrowBookAsync(int userId, int bookId);
        Task ReturnBookAsync(int userId, int bookId);
        Task<ReportResult> GenerateReportAsync();
        Task CalculateFineAsync();

        Task<IEnumerable<BorrowRecord>> GetAllBorrowRecordsAsync();
       Task<IEnumerable<Fine>> GetAllFinesAsync();
    }
}