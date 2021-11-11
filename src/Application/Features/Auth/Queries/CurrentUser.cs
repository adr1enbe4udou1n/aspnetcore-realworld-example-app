using Application.Interfaces;
using AutoMapper;

namespace Application.Features.Auth.Queries;

public class UserDTO
{
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? Image { get; set; }

    public string Token { get; set; } = string.Empty;
}

public record UserResponse(UserDTO User);

public record CurrentUserQuery() : IAuthorizationRequest<UserResponse>;

public class CurrentUserHandler : IAuthorizationRequestHandler<CurrentUserQuery, UserResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public CurrentUserHandler(ICurrentUser currentUser, IMapper mapper)
    {
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public Task<UserResponse> Handle(CurrentUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserResponse(
            _mapper.Map<UserDTO>(_currentUser.User)
        ));
    }
}