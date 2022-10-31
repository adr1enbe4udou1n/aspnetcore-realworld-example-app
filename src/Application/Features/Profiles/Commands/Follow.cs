using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Profiles.Commands;

public record ProfileFollowRequest(string Username, bool Follow) : ICommand<ProfileResponse>;

public class ProfileGetHandler : ICommandHandler<ProfileFollowRequest, ProfileResponse>
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
            .Include(u => u.Followers)
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        if (request.Follow)
        {
            user.Follow(_currentUser.User!);
        }
        else
        {
            user.Unfollow(_currentUser.User!);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileResponse(_mapper.Map<ProfileDTO>(user));
    }
}