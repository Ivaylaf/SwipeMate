namespace SwipeMate.Api.Dtos;

public class CreateSessionDto
{
    public string Category { get; set; } = "Movie";
    public List<string> FriendUserNames { get; set; } = new();
}

