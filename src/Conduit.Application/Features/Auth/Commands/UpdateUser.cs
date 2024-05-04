using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Auth.Commands;

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? Image { get; set; }
}

public record UpdateUserCommand(UpdateUserDto User) : IRequest<UserResponse>;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator(ICurrentUser currentUser, IAppDbContext context)
    {
        RuleFor(x => x.User.Username).NotEmpty().When(x => x.User.Username != null);

        When(x => !string.IsNullOrEmpty(x.User.Email), () =>
        {
            RuleFor(x => x.User.Email).EmailAddress();

            RuleFor(x => x.User.Email).MustAsync(
                async (email, cancellationToken) => !await context.Users
                    .Where(x => x.Id != currentUser.User!.Id && x.Email == email)
                    .AnyAsync(cancellationToken)
                )
                    .WithMessage("Email is already used");
        });
    }
}

public class UpdateUserHandler(ICurrentUser currentUser, IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<UpdateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = currentUser.User!;

        user.Name = request.User.Username ?? user.Name;
        user.Email = request.User.Email ?? user.Email;
        user.Bio = request.User.Bio ?? user.Bio;
        user.Image = request.User.Image ?? user.Image;

        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(jwtTokenGenerator));
    }
}