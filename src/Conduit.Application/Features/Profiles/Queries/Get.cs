using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Features.Profiles.Queries;

public class ProfileDto
{
    public required string Username { get; set; }

    public string? Bio { get; set; }

    public string? Image { get; set; }

    public bool Following { get; set; }
}

public record ProfileResponse(ProfileDto Profile);

public record ProfileGetQuery(string Username) : IRequest<ProfileResponse>;

public class ProfileGetHandler : IRequestHandler<ProfileGetQuery, ProfileResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ProfileGetHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProfileResponse> Handle(ProfileGetQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FindAsync(x => x.Name == request.Username, cancellationToken);

        return new ProfileResponse(user.MapToProfile(_currentUser.User));
    }
}