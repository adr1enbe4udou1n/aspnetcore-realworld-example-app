using Application.Features.Auth.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class NewUserDTO
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Username { get; set; } = default!;
}

public record NewUserRequest(NewUserDTO User) : ICommand<UserResponse>;

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
    private readonly IMapper _mapper;

    public RegisterHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(NewUserRequest request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<NewUserDTO, User>(request.User);

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse(_mapper.Map<UserDTO>(user));
    }
}