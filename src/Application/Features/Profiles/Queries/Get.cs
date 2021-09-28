using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using AutoMapper;
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

        public ProfileGetHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProfileResponse> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .FindAsync(x => x.Name == request.Username, cancellationToken);

            return new ProfileResponse(_mapper.Map<ProfileDTO>(user));
        }
    }
}
