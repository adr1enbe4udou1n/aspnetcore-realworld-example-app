using System.Threading;
using System.Threading.Tasks;
using Application.Features.Profiles.Queries;
using Application.Interfaces;

namespace Application.Features.Profiles.Commands
{
    public record ProfileFollowCommand(string Username, bool Follow) : IAuthorizationRequest<ProfileEnvelope>;

    public class ProfileGetHandler : IAuthorizationRequestHandler<ProfileFollowCommand, ProfileEnvelope>
    {
        private readonly IAppDbContext _context;

        public ProfileGetHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<ProfileEnvelope> Handle(ProfileFollowCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}