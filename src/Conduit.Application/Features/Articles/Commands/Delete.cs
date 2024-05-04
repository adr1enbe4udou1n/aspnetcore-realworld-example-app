using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Features.Articles.Commands;

public record ArticleDeleteCommand(string Slug) : IRequest;

public class ArticleDeleteHandler(IAppDbContext context, ICurrentUser currentUser) : IRequestHandler<ArticleDeleteCommand>
{
    public async Task Handle(ArticleDeleteCommand request, CancellationToken cancellationToken)
    {
        var article = await context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (article.AuthorId != currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        context.Articles.Remove(article);
        await context.SaveChangesAsync(cancellationToken);
    }
}