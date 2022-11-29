using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Domain.Entities;

namespace Application.Features.Auth.Queries;

public class UserDTO
{
    public string Email { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string Bio { get; set; } = default!;

    public string Image { get; set; } = default!;

    public string Token { get; set; } = default!;
}

public static class UserMapper
{
    public static UserDTO Map(this User user, IJwtTokenGenerator jwtTokenGenerator)
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

    public static ProfileDTO MapToProfile(this User user, User? currentUser)
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

public record UserResponse(UserDTO User);

public record CurrentUserQuery() : IQuery<UserResponse>;

public class CurrentUserHandler : IQueryHandler<CurrentUserQuery, UserResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public CurrentUserHandler(ICurrentUser currentUser, IJwtTokenGenerator jwtTokenGenerator)
    {
        _currentUser = currentUser;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public Task<UserResponse> Handle(CurrentUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserResponse(
            _currentUser.User!.Map(_jwtTokenGenerator)
        ));
    }
}