using Application.Features.Auth.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class LoginUserDTO
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;
}

public record LoginUserRequest(LoginUserDTO User) : ICommand<UserResponse>;

public class LoginHandler : ICommandHandler<LoginUserRequest, UserResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public LoginHandler(IAppDbContext context, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Where(x => x.Email == request.User.Email)
            .SingleOrDefaultAsync(cancellationToken);

        if (user == null || user.Password == null || !_passwordHasher.Check(request.User.Password, user.Password))
        {
            throw new Exceptions.ValidationException("Bad credentials");
        }

        return new UserResponse(_mapper.Map<UserDTO>(user));
    }
}