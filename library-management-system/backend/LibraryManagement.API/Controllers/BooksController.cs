using LibraryManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET /api/books — all books available in the library 
    [HttpGet]
    public async Task<IActionResult> GetAvailableBooks()
    {
        var books = await _bookService.GetAvailableBooksAsync();
        return Ok(books);
    }

    // GET /api/books/my-books — books currently checked out by the logged-in user
    [HttpGet("my-books")]
    public async Task<IActionResult> GetMyBooks()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var books = await _bookService.GetUserBooksAsync(userId);
        return Ok(books);
    }

    // POST /api/books/{id}/checkout — borrow a book
    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> Checkout(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _bookService.CheckoutBookAsync(id, userId);
        if (!success)
            return BadRequest(new { message = "Book is currently unavailable or already borrowed." });
        return Ok(new { message = "Book checked out successfully." });
    }

    // POST /api/books/{id}/return — return a borrowed book
    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _bookService.ReturnBookAsync(id, userId);
        if (!success)
            return BadRequest(new { message = "You don't have this book." });
        return Ok(new { message = "Book returned successfully." });
    }
}
