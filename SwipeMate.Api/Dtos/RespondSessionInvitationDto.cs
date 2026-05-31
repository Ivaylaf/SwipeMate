namespace SwipeMate.Api.Dtos;

public class RespondSessionInvitationDto
{
    public Guid InvitationId { get; set; }
    public bool Accept { get; set; }
}

