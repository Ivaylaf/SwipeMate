using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class FriendshipRequest
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string FromUserId { get; set; } = default!;

    [Required]
    public string ToUserId { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "Pending";
}


