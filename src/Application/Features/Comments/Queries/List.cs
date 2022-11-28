using Application.Extensions;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
using Application.Interfaces.Mediator;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries;

public class CommentDTO
{
    public CommentDTO()
    {
    }

    public CommentDTO(Comment comment, User? user)
    {
        Id = comment.Id;
        Body = comment.Body;
        CreatedAt = comment.CreatedAt;
        UpdatedAt = comment.UpdatedAt;
        Author = new ProfileDTO(comment.Author, user);
    }

    public int Id { get; private set; }

    public string Body { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ProfileDTO Author { get; set; } = null!;
}

public record MultipleCommentsResponse(IEnumerable<CommentDTO> Comments);

public record CommentsListQuery(string Slug) : IQuery<MultipleCommentsResponse>;

public class CommentsListHandler : IQueryHandler<CommentsListQuery, MultipleCommentsResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CommentsListHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MultipleCommentsResponse> Handle(CommentsListQuery request, CancellationToken cancellationToken)
    {
        await _currentUser.LoadFollowing();

        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.ArticleId == article.Id)
            .OrderByDescending(x => x.Id)
            .Select(c => new CommentDTO(c, _currentUser.User))
            .ToListAsync(cancellationToken);

        return new MultipleCommentsResponse(comments);
    }
}