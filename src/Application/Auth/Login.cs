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
    public class Login
    {
        public class CredentialsDTO
        {
            public string Email { get; set; }

            public string Password { get; set; }
        }

        public record Command(CredentialsDTO User) : IRequest<UserEnvelope>;

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IAppDbContext context, IPasswordHasher passwordHasher)
            {
                RuleFor(x => x.User.Email).NotNull().NotEmpty().EmailAddress();
                RuleFor(x => x.User.Password).NotNull().NotEmpty().MinimumLength(8);

                RuleFor(x => x.User.Email).Must(
                    (credentials, email) =>
                    {
                        var user = context.Users.Where(x => x.Email == email).SingleOrDefault();

                        return user != null && passwordHasher.Check(credentials.User.Password, user.Password);
                    }
                )
                    .WithMessage("Bad credentials");
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
                var user = await _context.Users.Where(x => x.Email == request.User.Email).SingleOrDefaultAsync();

                var currentUser = _mapper.Map<User, CurrentUser>(user);
                currentUser.Token = _jwtTokenGenerator.CreateToken(user);
                return new UserEnvelope(currentUser);
            }
        }
    }
}