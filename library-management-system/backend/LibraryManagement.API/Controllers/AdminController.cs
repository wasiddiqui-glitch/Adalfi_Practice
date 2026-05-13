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

    // GET /api/admin/users — all users with their current checkouts
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersWithBooksAsync();
        return Ok(users);
    }

    // GET /api/admin/checkouts — all active checkouts across all users
    [HttpGet("checkouts")]
    public async Task<IActionResult> GetAllCheckouts()
    {
        var checkouts = await _adminService.GetAllActiveCheckoutsAsync();
        return Ok(checkouts);
    }

    // POST /api/admin/copies/{id}/mark-faulty — admin marks a specific copy as faulty
    [HttpPost("copies/{id}/mark-faulty")]
    public async Task<IActionResult> MarkCopyFaulty(int id, [FromBody] MarkFaultyRequest request)
    {
        var success = await _bookService.MarkCopyFaultyAsync(id, request.Reason);
        if (!success) return NotFound(new { message = "Copy not found." });
        return Ok(new { message = "Copy marked as faulty." });
    }

    // POST /api/admin/copies/{id}/restore — admin restores a faulty copy
    [HttpPost("copies/{id}/restore")]
    public async Task<IActionResult> RestoreCopy(int id)
    {
        var success = await _bookService.RestoreCopyAsync(id);
        if (!success) return NotFound(new { message = "Copy not found." });
        return Ok(new { message = "Copy restored to circulation." });
    }
}
