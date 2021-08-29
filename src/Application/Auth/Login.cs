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
    public class LoginDTO
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public record LoginCommand(LoginDTO User) : IRequest<UserEnvelope>;

    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.User.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.User.Password).NotNull().NotEmpty().MinimumLength(8);
        }
    }

    public class LoginHandler : IRequestHandler<LoginCommand, UserEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;

        public LoginHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
        }

        public async Task<UserEnvelope> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.Where(x => x.Email == request.User.Email).SingleOrDefaultAsync();

            if (user == null || !_passwordHasher.Check(request.User.Password, user.Password))
            {
                throw new ValidationException("Bad credentials");
            }

            var currentUser = _mapper.Map<User, CurrentUser>(user);
            currentUser.Token = _jwtTokenGenerator.CreateToken(user);
            return new UserEnvelope(currentUser);
        }
    }
}