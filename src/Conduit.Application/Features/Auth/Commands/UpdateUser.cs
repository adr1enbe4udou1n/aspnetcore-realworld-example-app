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

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UpdateUserHandler(ICurrentUser currentUser, IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _currentUser = currentUser;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUser.User!;

        user.Name = request.User.Username ?? user.Name;
        user.Email = request.User.Email ?? user.Email;
        user.Bio = request.User.Bio ?? user.Bio;
        user.Image = request.User.Image ?? user.Image;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Map(_jwtTokenGenerator));
    }
}