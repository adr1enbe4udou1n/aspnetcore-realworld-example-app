using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Auth.Commands;

public class RegisterValidator : AbstractValidator<NewUserDto>
{
    public RegisterValidator(IAppDbContext context)
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8);
        RuleFor(x => x.Username).NotNull().NotEmpty();

        RuleFor(x => x.Email).MustAsync(
            async (email, cancellationToken) => !await context.Users
                .Where(x => x.Email == email)
                .AnyAsync(cancellationToken)
        )
            .WithMessage("Email is already used");
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator(ICurrentUser currentUser, IAppDbContext context)
    {
        RuleFor(x => x.Username).NotEmpty().When(x => x.Username != null);

        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email).EmailAddress();

            RuleFor(x => x.Email).MustAsync(
                async (email, cancellationToken) => !await context.Users
                    .Where(x => x.Id != currentUser.User!.Id && x.Email == email)
                    .AnyAsync(cancellationToken)
                )
                    .WithMessage("Email is already used");
        });
    }
}

public class CommandUsers(ICurrentUser currentUser, IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IValidator<NewUserDto> registerValidator, IValidator<UpdateUserDto> updateValidator) : ICommandUsers
{
    public async Task<UserResponse> Login(LoginUserDto credentials, CancellationToken cancellationToken)
    {
        var user = await context.Users.Where(x => x.Email == credentials.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user?.Password is null || !passwordHasher.Check(credentials.Password, user.Password))
        {
            throw new Exceptions.ValidationException("Bad credentials");
        }

        return new UserResponse(user.Map(jwtTokenGenerator));
    }

    public async Task<UserResponse> Register(NewUserDto newUser, CancellationToken cancellationToken)
    {
        var result = await registerValidator.ValidateAsync(newUser, cancellationToken);
        if (!result.IsValid)
            throw new Exceptions.ValidationException(result.Errors);

        var user = new User
        {
            Name = newUser.Username,
            Email = newUser.Email,
            Password = passwordHasher.Hash(newUser.Password)
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(jwtTokenGenerator));
    }

    public async Task<UserResponse> Update(UpdateUserDto updateUser, CancellationToken cancellationToken)
    {
        var result = await updateValidator.ValidateAsync(updateUser, cancellationToken);
        if (!result.IsValid)
            throw new Exceptions.ValidationException(result.Errors);

        var user = currentUser.User!;

        user.Name = updateUser.Username ?? user.Name;
        user.Email = updateUser.Email ?? user.Email;
        user.Bio = updateUser.Bio ?? user.Bio;
        user.Image = updateUser.Image ?? user.Image;

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(jwtTokenGenerator));
    }
}