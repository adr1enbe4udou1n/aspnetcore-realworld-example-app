using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auth.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands
{
    public class UpdateUserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
    }

    public record UpdateUserRequest(UpdateUserDTO User) : IAuthorizationRequest<UserResponse>;

    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator(ICurrentUser currentUser, IAppDbContext context)
        {
            RuleFor(x => x.User.Username).NotEmpty().When(x => x.User.Username != null);

            When(x => !string.IsNullOrEmpty(x.User.Email), () =>
            {
                RuleFor(x => x.User.Email).EmailAddress();

                RuleFor(x => x.User.Email).MustAsync(
                    async (email, cancellationToken) => !await context.Users
                        .Where(x => x.Id != currentUser.User.Id && x.Email == email)
                        .AnyAsync(cancellationToken)
                    )
                        .WithMessage("Email is already used");
            });
        }
    }

    public class UpdateUserHandler : IAuthorizationRequestHandler<UpdateUserRequest, UserResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public UpdateUserHandler(ICurrentUser currentUser, IAppDbContext context, IMapper mapper)
        {
            _currentUser = currentUser;
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserResponse> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<UpdateUserDTO, User>(request.User, _currentUser.User);
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return new UserResponse(_mapper.Map<UserDTO>(user));
        }
    }
}
