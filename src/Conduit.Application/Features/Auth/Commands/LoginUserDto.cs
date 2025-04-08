namespace Conduit.Application.Features.Auth.Commands;

public class LoginUserDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

