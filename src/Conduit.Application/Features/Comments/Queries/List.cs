using Conduit.Application.Extensions;
using Conduit.Application.Features.Auth.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Comments.Queries;

public class CommentDto
{
    public int Id { get; set; }

    public required string Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required ProfileDto Author { get; set; }
}

public static class CommentMapper
{
    public static CommentDto Map(this Comment comment, User? user)
    {
        return new()
        {
            Id = comment.Id,
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Author = comment.Author.MapToProfile(user),
        };
    }
}

public record MultipleCommentsResponse(IEnumerable<CommentDto> Comments);

public record CommentsListQuery(string Slug) : IRequest<MultipleCommentsResponse>;

public class CommentsListHandler : IRequestHandler<CommentsListQuery, MultipleCommentsResponse>
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
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.ArticleId == article.Id)
            .OrderByDescending(x => x.Id)
            .Select(c => c.Map(_currentUser.User))
            .ToListAsync(cancellationToken);

        return new MultipleCommentsResponse(comments);
    }
}