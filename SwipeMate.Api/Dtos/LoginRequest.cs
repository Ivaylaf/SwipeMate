namespace SwipeMate.Api.Dtos;

public class LoginRequest
{
    public string UserNameOrEmail { get; set; } = default!;
    public string Password { get; set; } = default!;
}


