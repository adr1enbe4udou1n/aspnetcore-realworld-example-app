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

public class LoginHandler : IRequestHandler<LoginUserCommand, UserResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Where(x => x.Email == request.User.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user?.Password is null || !_passwordHasher.Check(request.User.Password, user.Password))
        {
            throw new Exceptions.ValidationException("Bad credentials");
        }

        return new UserResponse(user.Map(_jwtTokenGenerator));
    }
}