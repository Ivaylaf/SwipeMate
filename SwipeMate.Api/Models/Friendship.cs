using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class Friendship
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserAId { get; set; } = default!;

    [Required]
    public string UserBId { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

