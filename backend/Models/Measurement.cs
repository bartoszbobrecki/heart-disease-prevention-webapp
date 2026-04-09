using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardioBackend.Models;

public class Measurement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int? HeartRate { get; set; }
    public int? Systolic { get; set; }
    public int? Diastolic { get; set; }
    public int? ActivityMinutes { get; set; }

    public string? Notes { get; set; }
}
