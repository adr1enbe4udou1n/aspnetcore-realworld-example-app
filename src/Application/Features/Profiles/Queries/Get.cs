using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Application.Features.Profiles.Queries
{
    public class ProfileDTO
    {

        public string Username { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }
        public bool Following { get; set; }
    }

    public record ProfileEnvelope(ProfileDTO Profile);

    public record ProfileGetQuery(string username) : IAuthorizationRequest<ProfileEnvelope>;

    public class ProfileGetHandler : IAuthorizationRequestHandler<ProfileGetQuery, ProfileEnvelope>
    {
        public Task<ProfileEnvelope> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}