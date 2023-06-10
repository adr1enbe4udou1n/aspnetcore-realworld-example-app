using Conduit.Application.Exceptions;
using Conduit.Application.Extensions;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Interfaces;

using FluentValidation;

using MediatR;

namespace Conduit.Application.Features.Articles.Commands;

public class UpdateArticleDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Body { get; set; }
}

public record UpdateArticleCommand(string Slug, UpdateArticleDto Article) : IRequest<SingleArticleResponse>;

public class ArticleUpdateValidator : AbstractValidator<UpdateArticleCommand>
{
    public ArticleUpdateValidator()
    {
        RuleFor(x => x.Article.Title).NotEmpty().When(x => x.Article.Title != null);
        RuleFor(x => x.Article.Description).NotEmpty().When(x => x.Article.Description != null);
        RuleFor(x => x.Article.Body).NotEmpty().When(x => x.Article.Body != null);
    }
}

public class ArticleUpdateHandler : IRequestHandler<UpdateArticleCommand, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ArticleUpdateHandler(IAppDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SingleArticleResponse> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (article.AuthorId != _currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        article.Title = request.Article.Title ?? article.Title;
        article.Description = request.Article.Description ?? article.Description;
        article.Body = request.Article.Body ?? article.Body;

        _context.Articles.Update(article);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(article.Map(_currentUser.User));
    }
}