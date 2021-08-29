using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth
{
    public class Register
    {
        public class UserDTO
        {
            public string Email { get; set; }

            public string Password { get; set; }

            public string Username { get; set; }
        }

        public record Command(UserDTO User) : IRequest<UserEnvelope>;

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IAppDbContext context)
            {
                RuleFor(x => x.User.Email).NotNull().NotEmpty().EmailAddress();
                RuleFor(x => x.User.Password).NotNull().NotEmpty();
                RuleFor(x => x.User.Username).NotNull().NotEmpty();

                RuleFor(x => x.User.Email).Must(
                    email => !context.Users.Where(x => x.Email == email).Any()
                )
                    .WithMessage("Email must be unique");
            }
        }

        public class Handler : IRequestHandler<Command, UserEnvelope>
        {
            private readonly IAppDbContext _context;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;
            private readonly IMapper _mapper;

            public Handler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
            {
                _context = context;
                _passwordHasher = passwordHasher;
                _jwtTokenGenerator = jwtTokenGenerator;
                _mapper = mapper;
            }

            public async Task<UserEnvelope> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = new User
                {
                    Name = request.User.Username,
                    Email = request.User.Email,
                    Password = _passwordHasher.Hash(request.User.Password),
                };

                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var currentUser = _mapper.Map<User, CurrentUser>(user);
                currentUser.Token = _jwtTokenGenerator.CreateToken(user.Name);
                return new UserEnvelope(currentUser);
            }
        }
    }
}