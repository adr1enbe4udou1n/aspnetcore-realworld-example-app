using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Auth.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands
{
    public class LoginUserDTO
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public record LoginUserRequest(LoginUserDTO User) : IRequest<UserResponse>;

    public class LoginHandler : IRequestHandler<LoginUserRequest, UserResponse>
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

            if (user == null || !_passwordHasher.Check(request.User.Password, user.Password))
            {
                throw new ValidationException("Bad credentials");
            }

            return new UserResponse(_mapper.Map<UserDTO>(user));
        }
    }
}
