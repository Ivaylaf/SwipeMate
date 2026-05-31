namespace SwipeMate.Api.Dtos;

public class RegisterRequest
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? DisplayName { get; set; }
}

