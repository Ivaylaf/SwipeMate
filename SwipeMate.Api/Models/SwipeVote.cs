using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class SwipeVote
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public bool IsYes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
