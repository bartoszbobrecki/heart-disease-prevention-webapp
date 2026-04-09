using System.ComponentModel.DataAnnotations;

namespace CardioBackend.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
