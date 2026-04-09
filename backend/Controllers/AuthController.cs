using Microsoft.AspNetCore.Mvc;
using CardioBackend.Data;
using CardioBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
// ...existing using directives...

namespace CardioBackend.Controllers;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return Conflict("Email in use");
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Name = dto.Name,
            Age = dto.Age,
            Sex = dto.Sex,
            Cholesterol = dto.Cholesterol,
            Smoker = dto.Smoker
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMe), new { }, new { user.Id, user.Email, user.Name });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return Unauthorized();
        var token = GenerateJwt(user);
        // create refresh token
        var refresh = CreateRefreshToken();
        var refreshModel = new RefreshToken { UserId = user.Id, Token = refresh, ExpiresAt = DateTime.UtcNow.AddDays(30) };
        _db.RefreshTokens.Add(refreshModel);
        await _db.SaveChangesAsync();
        return Ok(new { accessToken = token, refreshToken = refresh });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);
        if (rt == null || rt.Revoked || rt.ExpiresAt <= DateTime.UtcNow) return Unauthorized();

        var user = await _db.Users.FindAsync(rt.UserId);
        if (user == null) return Unauthorized();

        // revoke current refresh token and issue a new one (rotation)
        rt.Revoked = true;
        var newRefresh = CreateRefreshToken();
        var newRt = new RefreshToken { UserId = user.Id, Token = newRefresh, ExpiresAt = DateTime.UtcNow.AddDays(30) };
        _db.RefreshTokens.Add(newRt);
        await _db.SaveChangesAsync();

        var access = GenerateJwt(user);
        return Ok(new { accessToken = access, refreshToken = newRefresh });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeDto dto)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == dto.RefreshToken);
        if (rt == null) return NotFound();
        rt.Revoked = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (sub == null) return Unauthorized();
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();
        return Ok(new { user.Id, user.Email, user.Name, user.CreatedAt });
    }

    private string GenerateJwt(User user)
    {
        var key = _config["Jwt:Key"] ?? "ThisIsADevSigningKeyReplaceInProd";
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email) }),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string CreateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    // using BCrypt for password hashing (handled above)
}

public record RegisterDto(string Email, string Password, string? Name, int? Age, string? Sex, int? Cholesterol, bool? Smoker);
public record LoginDto(string Email, string Password);
public record RefreshRequestDto(string RefreshToken);
public record RevokeDto(string RefreshToken);
