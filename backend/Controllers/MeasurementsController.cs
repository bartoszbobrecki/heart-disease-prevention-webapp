using Microsoft.AspNetCore.Mvc;
using CardioBackend.Data;
using CardioBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CardioBackend.Controllers;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeasurementsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MeasurementsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementDto dto)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return Unauthorized();

        var meas = new Measurement
        {
            UserId = userId,
            Timestamp = dto.Timestamp ?? DateTime.UtcNow,
            HeartRate = dto.HeartRate,
            Systolic = dto.Systolic,
            Diastolic = dto.Diastolic,
            ActivityMinutes = dto.ActivityMinutes,
            Notes = dto.Notes
        };
        _db.Measurements.Add(meas);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = meas.Id }, meas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var m = await _db.Measurements.FindAsync(id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [HttpGet("me")]
    public async Task<IActionResult> ListForUser(DateTime? from, DateTime? to)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return Unauthorized();

        var q = _db.Measurements.Where(m => m.UserId == userId);
        if (from.HasValue) q = q.Where(m => m.Timestamp >= from.Value);
        if (to.HasValue) q = q.Where(m => m.Timestamp <= to.Value);
        var list = await q.OrderByDescending(m => m.Timestamp).Take(1000).ToListAsync();
        return Ok(list);
    }
}

public record CreateMeasurementDto(DateTime? Timestamp, int? HeartRate, int? Systolic, int? Diastolic, int? ActivityMinutes, string? Notes);
