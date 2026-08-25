using System.ComponentModel.DataAnnotations;

namespace Conduit.Application.Features.Auth.Commands;

public class NewUserDto
{
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }

    public required string Username { get; set; }
}
