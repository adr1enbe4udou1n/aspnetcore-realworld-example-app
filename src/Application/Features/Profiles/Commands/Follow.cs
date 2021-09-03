using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Profiles.Commands
{
    public record ProfileFollowCommand(string Username, bool Follow) : IAuthorizationRequest<ProfileEnvelope>;

    public class ProfileGetHandler : IAuthorizationRequestHandler<ProfileFollowCommand, ProfileEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ProfileGetHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ProfileEnvelope> Handle(ProfileFollowCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(x => x.Name == request.Username, cancellationToken);

            if (request.Follow)
            {
                if (!_currentUser.User.IsFollowing(user))
                {
                    _currentUser.User.Following.Add(new FollowerUser { Following = user });
                }
            }
            else
            {
                if (_currentUser.User.IsFollowing(user))
                {
                    _currentUser.User.Following.RemoveAll(x => x.FollowingId == user.Id);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ProfileEnvelope(_mapper.Map<User, ProfileDTO>(user));
        }
    }
}