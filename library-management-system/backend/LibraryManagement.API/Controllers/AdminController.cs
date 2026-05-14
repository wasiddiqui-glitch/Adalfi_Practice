using LibraryManagement.API.DTOs;
using LibraryManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IBookService _bookService;

    public AdminController(IAdminService adminService, IBookService bookService)
    {
        _adminService = adminService;
        _bookService = bookService;
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersWithBooksAsync();
        return Ok(users);
    }

    // GET /api/admin/checkouts
    [HttpGet("checkouts")]
    public async Task<IActionResult> GetAllCheckouts()
    {
        var checkouts = await _adminService.GetAllActiveCheckoutsAsync();
        return Ok(checkouts);
    }

    // GET /api/admin/checkouts/overdue
    [HttpGet("checkouts/overdue")]
    public async Task<IActionResult> GetOverdueCheckouts()
    {
        var checkouts = await _adminService.GetOverdueCheckoutsAsync();
        return Ok(checkouts);
    }

    // POST /api/admin/copies/{id}/mark-faulty
    [HttpPost("copies/{id}/mark-faulty")]
    public async Task<IActionResult> MarkCopyFaulty(int id, [FromBody] MarkFaultyRequest request)
    {
        var success = await _bookService.MarkCopyFaultyAsync(id, request.Reason);
        if (!success) return NotFound(new { message = "Copy not found." });
        return Ok(new { message = "Copy marked as faulty." });
    }

    // POST /api/admin/copies/{id}/restore
    [HttpPost("copies/{id}/restore")]
    public async Task<IActionResult> RestoreCopy(int id)
    {
        var success = await _bookService.RestoreCopyAsync(id);
        if (!success) return NotFound(new { message = "Copy not found." });
        return Ok(new { message = "Copy restored to circulation." });
    }

    // GET /api/admin/books/{id}
    [HttpGet("books/{id}")]
    public async Task<IActionResult> GetBookDetail(int id)
    {
        var book = await _adminService.GetBookDetailAsync(id);
        if (book == null) return NotFound(new { message = "Book not found." });
        return Ok(book);
    }

    // POST /api/admin/books
    [HttpPost("books")]
    public async Task<IActionResult> AddBook([FromBody] CreateBookRequest request)
    {
        var book = await _adminService.AddBookAsync(request);
        return Ok(book);
    }

    // PUT /api/admin/books/{id}
    [HttpPut("books/{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        var book = await _adminService.UpdateBookAsync(id, request);
        if (book == null) return NotFound(new { message = "Book not found." });
        return Ok(book);
    }

    // DELETE /api/admin/books/{id}
    [HttpDelete("books/{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var (success, error) = await _adminService.DeleteBookAsync(id);
        if (!success) return BadRequest(new { message = error });
        return Ok(new { message = "Book deleted." });
    }

    // POST /api/admin/books/{id}/copies
    [HttpPost("books/{id}/copies")]
    public async Task<IActionResult> AddCopy(int id)
    {
        var copy = await _adminService.AddCopyAsync(id);
        if (copy == null) return NotFound(new { message = "Book not found." });
        return Ok(copy);
    }

    // DELETE /api/admin/copies/{id}
    [HttpDelete("copies/{id}")]
    public async Task<IActionResult> DeleteCopy(int id)
    {
        var (success, error) = await _adminService.DeleteCopyAsync(id);
        if (!success) return BadRequest(new { message = error });
        return Ok(new { message = "Copy deleted." });
    }
}
