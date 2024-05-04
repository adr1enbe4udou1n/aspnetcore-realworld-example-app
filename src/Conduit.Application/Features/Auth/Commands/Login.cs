using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Auth.Commands;

public class LoginUserDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public record LoginUserCommand(LoginUserDto User) : IRequest<UserResponse>;

public class LoginHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.Where(x => x.Email == request.User.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user?.Password is null || !passwordHasher.Check(request.User.Password, user.Password))
        {
            throw new Exceptions.ValidationException("Bad credentials");
        }

        return new UserResponse(user.Map(jwtTokenGenerator));
    }
}