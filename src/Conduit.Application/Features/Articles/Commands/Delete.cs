using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Interfaces;

using MediatR;

namespace Conduit.Application.Features.Articles.Commands;

public record ArticleDeleteCommand(string Slug) : IRequest;

public class ArticleDeleteHandler : IRequestHandler<ArticleDeleteCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticleDeleteHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ArticleDeleteCommand request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (article.AuthorId != _currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync(cancellationToken);
    }
}