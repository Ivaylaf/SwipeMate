using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class MatchSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Category { get; set; } = default!;

    [Required]
    public string CreatedByUserId { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "Active";
}


