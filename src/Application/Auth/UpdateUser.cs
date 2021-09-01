using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth
{
    public class UpdateUserDTO
    {
        public string Email { get; set; }
    }

    public record UpdateUserCommand(UpdateUserDTO User) : IAuthorizationRequest<UserEnvelope>;

    public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserValidator(ICurrentUser currentUser, IAppDbContext context)
        {
            RuleFor(x => x.User.Email).NotNull().NotEmpty().EmailAddress();

            RuleFor(x => x.User.Email).MustAsync(
                async (email, cancellationToken) => !await context.Users
                    .Where(x => x.Id != currentUser.User.Id && x.Email == email)
                    .AnyAsync(cancellationToken)
            )
                .WithMessage("Email is already used");
        }
    }

    public class UpdateUserHandler : IAuthorizationRequestHandler<UpdateUserCommand, UserEnvelope>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IAppDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;

        public UpdateUserHandler(ICurrentUser currentUser, IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
        {
            _currentUser = currentUser;
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
        }

        public async Task<UserEnvelope> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            _currentUser.User.Email = request.User.Email;
            _context.Users.Update(_currentUser.User);
            await _context.SaveChangesAsync(cancellationToken);

            var user = _mapper.Map<User, CurrentUser>(_currentUser.User);
            user.Token = _jwtTokenGenerator.CreateToken(_currentUser.User);
            return new UserEnvelope(user);
        }
    }
}