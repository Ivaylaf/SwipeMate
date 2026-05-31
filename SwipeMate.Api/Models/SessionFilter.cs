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

    [Required]
    public string FilterJson { get; set; } = "{}";

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}


