using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class SessionMatch
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
