using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

namespace Conduit.Application.Features.Auth.Queries;

public class UserDto
{
    public required string Email { get; set; }

    public required string Username { get; set; }

    public string? Bio { get; set; }

    public string? Image { get; set; }

    public required string Token { get; set; }
}

public static class UserMapper
{
    public static UserDto Map(this User user, IJwtTokenGenerator jwtTokenGenerator)
    {
        return new()
        {
            Email = user.Email,
            Token = jwtTokenGenerator.CreateToken(user),
            Username = user.Name,
            Bio = user.Bio!,
            Image = user.Image!,
        };
    }

    public static ProfileDto MapToProfile(this User user, User? currentUser)
    {
        return new()
        {
            Username = user.Name,
            Bio = user.Bio,
            Image = user.Image,
            Following = currentUser != null && currentUser.IsFollowing(user),
        };
    }
}