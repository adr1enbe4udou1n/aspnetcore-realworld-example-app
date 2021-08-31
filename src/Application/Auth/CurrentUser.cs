using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Auth
{
    public class CurrentUser
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }

        public string Token { get; set; }
    }

    public record UserEnvelope(CurrentUser User);

    public record CurrentUserQuery() : IRequest<UserEnvelope>;

    public class CurrentUserHandler : IRequestHandler<CurrentUserQuery, UserEnvelope>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;

        public CurrentUserHandler(ICurrentUser currentUser, IJwtTokenGenerator jwtTokenGenerator, IMapper mapper)
        {
            _currentUser = currentUser;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mapper = mapper;
        }

        public Task<UserEnvelope> Handle(CurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User, CurrentUser>(_currentUser.User);
            user.Token = _jwtTokenGenerator.CreateToken(_currentUser.User);
            return Task.FromResult(new UserEnvelope(user));
        }
    }
}