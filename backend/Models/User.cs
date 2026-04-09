using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardioBackend.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }

    // Optional health-related fields
    public int? Age { get; set; }
    public string? Sex { get; set; }
    public int? Cholesterol { get; set; } // mg/dL
    public bool? Smoker { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
