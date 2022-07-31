using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries;

public class CommentDTO
{
    public int Id { get; set; }

    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ProfileDTO Author { get; set; } = null!;
}

public record MultipleCommentsResponse(IEnumerable<CommentDTO> Comments);

public record CommentsListQuery(string Slug) : IRequest<MultipleCommentsResponse>;

public class CommentsListHandler : IRequestHandler<CommentsListQuery, MultipleCommentsResponse>
{
    private readonly IAppRoDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public CommentsListHandler(IAppRoDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<MultipleCommentsResponse> Handle(CommentsListQuery request, CancellationToken cancellationToken)
    {
        await _currentUser.LoadFollowing();

        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comments = await _context.Comments
            .Where(c => c.ArticleId == article.Id)
            .OrderByDescending(x => x.Id)
            .ProjectTo<CommentDTO>(_mapper.ConfigurationProvider, new
            {
                currentUser = _currentUser.User
            })
            .ToListAsync(cancellationToken);

        return new MultipleCommentsResponse(comments);
    }
}