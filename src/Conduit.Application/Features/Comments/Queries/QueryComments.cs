using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Conduit.Application.Features.Comments.Queries;

public record MultipleCommentsResponse(IEnumerable<CommentDto> Comments);

public class QueryComments(IAppDbContext context, ICurrentUser currentUser) : IQueryComments
{
    public async Task<MultipleCommentsResponse> List(string slug, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == slug, cancellationToken);

        var comments = await context.Comments
            .Include(c => c.Author)
            .Where(c => c.ArticleId == article.Id)
            .OrderByDescending(x => x.Id)
            .Select(c => c.Map(currentUser.User))
            .ToListAsync(cancellationToken);

        return new MultipleCommentsResponse(comments);
    }
}