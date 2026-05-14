using LibraryManagement.API.DTOs;
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

    // GET /api/books — all books with copy availability counts
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookService.GetAvailableBooksAsync();
        return Ok(books);
    }

    // GET /api/books/faulty — books that have at least one faulty copy
    [HttpGet("faulty")]
    public async Task<IActionResult> GetFaultyBooks()
    {
        var books = await _bookService.GetFaultyBooksAsync();
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

    // POST /api/books/{id}/checkout — borrow a book (picks next available copy)
    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> Checkout(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _bookService.CheckoutBookAsync(id, userId);
        if (!success) return BadRequest(new { message = error });
        return Ok(new { message = "Book checked out successfully. Due in 14 days." });
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

    // POST /api/books/{id}/report-faulty — report the specific copy you have as faulty
    [HttpPost("{id}/report-faulty")]
    public async Task<IActionResult> ReportFaulty(int id, [FromBody] MarkFaultyRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _bookService.MarkFaultyAsync(id, userId, request.Reason);
        if (!success) return BadRequest(new { message = "You don't have this book checked out." });
        return Ok(new { message = "Copy reported as faulty. Thank you — an admin will review it." });
    }

    // GET /api/books/history — checkout history for the logged-in user
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var history = await _bookService.GetCheckoutHistoryAsync(userId);
        return Ok(history);
    }
}
