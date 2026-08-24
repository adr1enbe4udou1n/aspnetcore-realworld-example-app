using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using FluentValidation;

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
            .WithMessage("has already been taken")
            .WithErrorCode("Conflict");

        RuleFor(x => x.Username).MustAsync(
            async (username, cancellationToken) => !await context.Users
                .Where(x => x.Name == username)
                .AnyAsync(cancellationToken)
        )
            .WithMessage("has already been taken")
            .WithErrorCode("Conflict");
    }
}

public class LoginValidator : AbstractValidator<LoginUserDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator(ICurrentUser currentUser, IAppDbContext context)
    {
        RuleFor(x => x.Username).NotEmpty().When(x => x.UsernameSpecified);
        RuleFor(x => x.Email).NotEmpty().When(x => x.EmailSpecified);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).When(x => x.PasswordSpecified);

        When(x => x.EmailSpecified && !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email).EmailAddress();

            RuleFor(x => x.Email).MustAsync(
                async (email, cancellationToken) => !await context.Users
                    .Where(x => x.Id != currentUser.User!.Id && x.Email == email)
                    .AnyAsync(cancellationToken)
                )
                    .WithMessage("has already been taken")
                    .WithErrorCode("Conflict");
        });
    }
}

public class CommandUsers(ICurrentUser currentUser, IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IValidator<LoginUserDto> loginValidator, IValidator<NewUserDto> registerValidator, IValidator<UpdateUserDto> updateValidator) : ICommandUsers
{
    public async Task<UserResponse> Login(LoginUserDto credentials, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(credentials, cancellationToken);

        var user = await context.Users.Where(x => x.Email == credentials.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user?.Password is null || !passwordHasher.Check(credentials.Password, user.Password))
        {
            throw new ValidationException("Bad credentials");
        }

        return new UserResponse(user.Map(jwtTokenGenerator));
    }

    public async Task<UserResponse> Register(NewUserDto newUser, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(newUser, cancellationToken);

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
        await updateValidator.ValidateAndThrowAsync(updateUser, cancellationToken);

        var user = currentUser.User!;

        if (updateUser.UsernameSpecified)
        {
            user.Name = updateUser.Username!;
        }

        if (updateUser.EmailSpecified)
        {
            user.Email = updateUser.Email!;
        }

        if (updateUser.PasswordSpecified)
        {
            user.Password = passwordHasher.Hash(updateUser.Password!);
        }

        if (updateUser.BioSpecified)
        {
            user.Bio = string.IsNullOrEmpty(updateUser.Bio) ? null : updateUser.Bio;
        }

        if (updateUser.ImageSpecified)
        {
            user.Image = string.IsNullOrEmpty(updateUser.Image) ? null : updateUser.Image;
        }

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(jwtTokenGenerator));
    }
}
