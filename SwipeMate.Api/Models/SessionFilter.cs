using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class SessionFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    // JSON с филтрите на този потребител
    [Required]
    public string FilterJson { get; set; } = "{}";

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

