using System.Threading;
using System.Threading.Tasks;
using Application.Features.Profiles.Queries;
using Application.Interfaces;

namespace Application.Features.Profiles.Commands
{
    public record ProfileFollowCommand(string username, bool follow) : IAuthorizationRequest<ProfileEnvelope>;

    public class ProfileGetHandler : IAuthorizationRequestHandler<ProfileFollowCommand, ProfileEnvelope>
    {
        public Task<ProfileEnvelope> Handle(ProfileFollowCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}