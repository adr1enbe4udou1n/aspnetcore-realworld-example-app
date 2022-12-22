using Application.Features.Auth.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class NewUserDto
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Username { get; set; } = default!;
}

public record NewUserRequest(NewUserDto User) : ICommand<UserResponse>;

public class RegisterValidator : AbstractValidator<NewUserRequest>
{
    public RegisterValidator(IAppDbContext context)
    {
        RuleFor(x => x.User.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(x => x.User.Password).NotNull().NotEmpty().MinimumLength(8);
        RuleFor(x => x.User.Username).NotNull().NotEmpty();

        RuleFor(x => x.User.Email).MustAsync(
            async (email, cancellationToken) => !await context.Users
                .Where(x => x.Email == email)
                .AnyAsync(cancellationToken)
        )
            .WithMessage("Email is already used");
    }
}

public class RegisterHandler : ICommandHandler<NewUserRequest, UserResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserResponse> Handle(NewUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.User.Username,
            Email = request.User.Email,
            Password = _passwordHasher.Hash(request.User.Password)
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(_jwtTokenGenerator));
    }
}