using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OLMS.Api.Models;
using OLMS.Api.Models.DTOs;
using OLMS.Api.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] string? findAll)
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync(search, genre,findAll);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching books.");
                return StatusCode(500, new { message = "Something went wrong while fetching books." });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null) return NotFound(new { message = "Book not found." });
                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching book with ID {id}.");
                return StatusCode(500, new { message = "Something went wrong while fetching the book." });
            }
        }

        [HttpPost("{id}/borrow")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Borrow(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return BadRequest(new { message = "User identifier is missing." });

                int userId = int.Parse(userIdClaim.Value);
                await _bookService.BorrowBookAsync(userId, id);
                return Ok(new { message = "Book borrowed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error borrowing book ID {id}.");
                return StatusCode(500, new { message = "Could not borrow the book." });
            }
        }

        [HttpPost("{id}/return")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Return(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return BadRequest(new { message = "User identifier is missing." });

                int userId = int.Parse(userIdClaim.Value);
                await _bookService.ReturnBookAsync(userId, id);
                return Ok(new { message = "Book returned." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error returning book ID {id}.");
                return StatusCode(500, new { message = "Could not return the book." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromBody] BookCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Invalid book data." });

                var book = new Book
                {
                    Title = dto.Title,
                    Author = dto.Author,
                    Description = dto.Description ?? string.Empty,
                    Genre = dto.Genre ?? string.Empty,
                    CoverImageUrl = dto.CoverImageUrl ?? string.Empty,
                    PdfUrl = dto.PdfUrl ?? string.Empty,
                    TotalCopies = dto.TotalCopies,
                    AvailableCopies = dto.TotalCopies
                };

                await _bookService.AddBookAsync(book);
                return CreatedAtAction(nameof(Get), new { id = book.BookId }, new { message = "Book added.", book });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new book.");
                return StatusCode(500, new { message = "Could not add the book." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Invalid book update data." });

                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null) return NotFound(new { message = "Book not found." });

                int diff = dto.TotalCopies - book.TotalCopies;
                book.Title = dto.Title;
                book.Author = dto.Author;
                book.Description = dto.Description ?? string.Empty;
                book.Genre = dto.Genre ?? string.Empty;
                book.CoverImageUrl = dto.CoverImageUrl ?? string.Empty;
                book.PdfUrl = dto.PdfUrl ?? string.Empty;
                book.TotalCopies = dto.TotalCopies;
                book.AvailableCopies += diff;
                if (book.AvailableCopies < 0) book.AvailableCopies = 0;

                await _bookService.UpdateBookAsync(book);
                return Ok(new { message = "Book updated.", book });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating book ID {id}.");
                return StatusCode(500, new { message = "Could not update the book." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null) return NotFound(new { message = "Book not found." });

                await _bookService.DeleteBookAsync(id);
                return Ok(new { message = "Book deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting book ID {id}.");
                return StatusCode(500, new { message = "Could not delete the book." });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("borrow-records")]
        public async Task<IActionResult> GetBorrowRecords()
        {
            var records = await _bookService.GetAllBorrowRecordsAsync();
            return Ok(records);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("fines")]
        public async Task<IActionResult> GetAllFines()
        {
            var fines = await _bookService.GetAllFinesAsync();
            return Ok(fines);
        }


    }
}
