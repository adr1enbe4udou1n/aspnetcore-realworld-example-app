using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Profiles.Queries
{
    public class ProfileDTO
    {

        public string Username { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }
        public bool Following { get; set; }
    }

    public record ProfileResponse(ProfileDTO Profile);

    public record ProfileGetQuery(string Username) : IRequest<ProfileResponse>;

    public class ProfileGetHandler : IRequestHandler<ProfileGetQuery, ProfileResponse>
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

        public async Task<ProfileResponse> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .ProjectTo<ProfileDTO>(_mapper.ConfigurationProvider, new
                {
                    currentUser = _currentUser.User
                })
                .FindAsync(x => x.Username == request.Username, cancellationToken);

            return new ProfileResponse(user);
        }
    }
}
