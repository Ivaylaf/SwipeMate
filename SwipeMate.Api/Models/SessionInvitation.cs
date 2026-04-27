namespace SwipeMate.Api.Models;

public class SessionInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string InvitedUserId { get; set; } = default!;
    public string InvitedByUserId { get; set; } = default!;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAtUtc { get; set; }
}
