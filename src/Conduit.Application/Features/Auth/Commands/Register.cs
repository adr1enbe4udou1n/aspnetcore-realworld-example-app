using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Auth.Commands;

public class NewUserDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string Username { get; set; }
}

public record NewUserCommand(NewUserDto User) : IRequest<UserResponse>;

public class RegisterValidator : AbstractValidator<NewUserCommand>
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

public class RegisterHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<NewUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(NewUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.User.Username,
            Email = request.User.Email,
            Password = passwordHasher.Hash(request.User.Password)
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(jwtTokenGenerator));
    }
}