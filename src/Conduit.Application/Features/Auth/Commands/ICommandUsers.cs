using Conduit.Application.Features.Auth.Queries;

namespace Conduit.Application.Features.Auth.Commands;

public interface ICommandUsers
{
    Task<UserResponse> Login(LoginUserDto credentials, CancellationToken cancellationToken);
    Task<UserResponse> Register(NewUserDto newUser, CancellationToken cancellationToken);
    Task<UserResponse> Update(UpdateUserDto updateUser, CancellationToken cancellationToken);
}