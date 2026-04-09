using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CardioBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace CardioBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/stats/summary?from=2026-01-01&to=2026-02-01
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(DateTime? from, DateTime? to)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (sub == null || !Guid.TryParse(sub, out var userId)) return Unauthorized();

        var q = _db.Measurements.Where(m => m.UserId == userId);
        if (from.HasValue) q = q.Where(m => m.Timestamp >= from.Value);
        if (to.HasValue) q = q.Where(m => m.Timestamp <= to.Value);

    var list = await q.ToListAsync();
        if (list.Count == 0) return Ok(new { count = 0, message = "No measurements in range" });

    var user = await _db.Users.FindAsync(userId);

        double avgHr = list.Where(x => x.HeartRate.HasValue).Select(x => x.HeartRate!.Value).DefaultIfEmpty(0).Average();
        double avgSys = list.Where(x => x.Systolic.HasValue).Select(x => x.Systolic!.Value).DefaultIfEmpty(0).Average();
        double avgDia = list.Where(x => x.Diastolic.HasValue).Select(x => x.Diastolic!.Value).DefaultIfEmpty(0).Average();
        int totalActivity = list.Where(x => x.ActivityMinutes.HasValue).Select(x => x.ActivityMinutes!.Value).DefaultIfEmpty(0).Sum();
        int count = list.Count;

        // Simple informational risk score (0-100)
        int score = 0;
        // blood pressure contribution
        if (avgSys >= 160 || avgDia >= 100) score += 40;
        else if (avgSys >= 140 || avgDia >= 90) score += 30;
        else if (avgSys >= 130 || avgDia >= 85) score += 10;

        // activity contribution (recommend 150 minutes/week). Estimate weeks in range
        var rangeDays = (to ?? DateTime.UtcNow).Date - (from ?? list.Min(x => x.Timestamp)).Date;
        var weeks = Math.Max(1, rangeDays.TotalDays / 7.0);
        var avgWeeklyActivity = totalActivity / weeks;
        if (avgWeeklyActivity < 75) score += 30;
        else if (avgWeeklyActivity < 150) score += 10;

        // heart rate
        if (avgHr >= 100) score += 30;
        else if (avgHr >= 90) score += 10;

        // user-specific factors
        if (user != null)
        {
            if (user.Age.HasValue && user.Age.Value >= 65) score += 10;
            if (user.Cholesterol.HasValue && user.Cholesterol.Value >= 240) score += 15;
            if (user.Smoker.HasValue && user.Smoker.Value) score += 20;
        }

        score = Math.Min(100, score);

        string category = score >= 67 ? "High" : score >= 34 ? "Moderate" : "Low";

        var result = new
        {
            count,
            averageHeartRate = Math.Round(avgHr, 1),
            averageSystolic = Math.Round(avgSys, 1),
            averageDiastolic = Math.Round(avgDia, 1),
            totalActivityMinutes = totalActivity,
            averageWeeklyActivity = Math.Round(avgWeeklyActivity,1),
            riskScore = score,
            riskCategory = category,
            recommendations = GetRecommendations(score)
        };


        return Ok(result);
    }

    private static string[] GetRecommendations(int score)
    {
        var list = new List<string>();
        if (score >= 67)
        {
            list.Add("High risk indicators present — consult a physician for evaluation.");
            list.Add("Consider monitoring blood pressure more frequently and improving physical activity.");
        }
        else if (score >= 34)
        {
            list.Add("Moderate risk — monitor lifestyle factors and consider medical advice.");
            list.Add("Aim for at least 150 minutes of moderate activity per week.");
        }
        else
        {
            list.Add("Low informational risk — continue healthy lifestyle and regular monitoring.");
        }
        return list.ToArray();
    }
}
