using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class MatchSessionParticipant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}

