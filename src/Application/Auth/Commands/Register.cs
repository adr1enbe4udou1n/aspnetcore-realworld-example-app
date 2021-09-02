using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Auth.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands
{
    public class RegisterDTO
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }
    }

    public record RegisterCommand(RegisterDTO User) : IRequest<UserEnvelope>;

    public class RegisterValidator : AbstractValidator<RegisterCommand>
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

    public class RegisterHandler : IRequestHandler<RegisterCommand, UserEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public RegisterHandler(IAppDbContext context, IPasswordHasher passwordHasher, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<UserEnvelope> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Name = request.User.Username,
                Email = request.User.Email,
                Password = _passwordHasher.Hash(request.User.Password),
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new UserEnvelope(_mapper.Map<User, CurrentUser>(user));
        }
    }
}