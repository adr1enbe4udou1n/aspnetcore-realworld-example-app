namespace Conduit.Application.Features.Auth.Commands;

public class NewUserDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string Username { get; set; }
}
