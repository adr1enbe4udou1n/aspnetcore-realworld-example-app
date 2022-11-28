using Application.Interfaces;
using Application.Interfaces.Mediator;
using Domain.Entities;

namespace Application.Features.Auth.Queries;

public class UserDTO
{
    public UserDTO()
    {
    }

    public UserDTO(User user, IJwtTokenGenerator jwtTokenGenerator)
    {
        Email = user.Email;
        Token = jwtTokenGenerator.CreateToken(user);
        Username = user.Name;
        Bio = user.Bio!;
        Image = user.Image!;
    }

    public string Email { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string Bio { get; set; } = default!;

    public string Image { get; set; } = default!;

    public string Token { get; set; } = default!;
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
            new UserDTO(_currentUser.User!, _jwtTokenGenerator)
        ));
    }
}