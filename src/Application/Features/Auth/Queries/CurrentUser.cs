using Application.Interfaces;
using Application.Interfaces.Mediator;
using AutoMapper;

namespace Application.Features.Auth.Queries;

public class UserDTO
{
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