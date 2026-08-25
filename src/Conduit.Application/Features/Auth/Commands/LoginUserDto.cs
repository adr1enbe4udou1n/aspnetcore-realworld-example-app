using System.ComponentModel.DataAnnotations;

namespace Conduit.Application.Features.Auth.Commands;

public class LoginUserDto
{
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}