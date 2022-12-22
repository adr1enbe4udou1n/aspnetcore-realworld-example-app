using Application.Features.Auth.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class LoginUserDto
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;
}

public record LoginUserRequest(LoginUserDto User) : ICommand<UserResponse>;

public class LoginHandler : ICommandHandler<LoginUserRequest, UserResponse>
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

    public async Task<UserResponse> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Where(x => x.Email == request.User.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user == null || user.Password == null || !_passwordHasher.Check(request.User.Password, user.Password))
        {
            throw new Exceptions.ValidationException("Bad credentials");
        }

        return new UserResponse(user.Map(_jwtTokenGenerator));
    }
}