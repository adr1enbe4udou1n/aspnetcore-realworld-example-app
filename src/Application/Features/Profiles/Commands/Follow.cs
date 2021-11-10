using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Profiles.Commands;

public record ProfileFollowRequest(string Username, bool Follow) : IAuthorizationRequest<ProfileResponse>;

public class ProfileGetHandler : IAuthorizationRequestHandler<ProfileFollowRequest, ProfileResponse>
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

    public async Task<ProfileResponse> Handle(ProfileFollowRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsTracking()
            .Include(u => u.Followers)
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        if (request.Follow)
        {
            if (!user.IsFollowedBy(_currentUser.User))
            {
                user.Followers.Add(new FollowerUser { Follower = _currentUser.User });
            }
        }
        else
        {
            if (user.IsFollowedBy(_currentUser.User))
            {
                user.Followers.RemoveAll(x => x.FollowerId == _currentUser.User.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(_mapper.Map<ProfileDTO>(user));
    }
}
