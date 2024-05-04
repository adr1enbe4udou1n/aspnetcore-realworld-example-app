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

public class CommentsListHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<CommentsListQuery, MultipleCommentsResponse>
{
    public async Task<MultipleCommentsResponse> Handle(CommentsListQuery request, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        var comments = await context.Comments
            .Include(c => c.Author)
            .Where(c => c.ArticleId == article.Id)
            .OrderByDescending(x => x.Id)
            .Select(c => c.Map(currentUser.User))
            .ToListAsync(cancellationToken);

        return new MultipleCommentsResponse(comments);
    }
}