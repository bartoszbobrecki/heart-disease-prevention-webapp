using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CardioBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace CardioBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return Unauthorized();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();
        return Ok(new { user.Id, user.Email, user.Name, user.Age, user.Sex, user.Cholesterol, user.Smoker, user.CreatedAt });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return Unauthorized();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        user.Name = dto.Name ?? user.Name;
        user.Age = dto.Age ?? user.Age;
        user.Sex = dto.Sex ?? user.Sex;
        user.Cholesterol = dto.Cholesterol ?? user.Cholesterol;
        user.Smoker = dto.Smoker ?? user.Smoker;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated" });
    }
}

public record UpdateUserDto(string? Name, int? Age, string? Sex, int? Cholesterol, bool? Smoker);
