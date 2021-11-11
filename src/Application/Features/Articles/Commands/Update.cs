using System.ComponentModel;
using Application.Exceptions;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Articles.Commands;

public class UpdateArticleDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Body { get; set; }
}

[DisplayName("UpdateArticleRequest")]
public record UpdateArticleBody(UpdateArticleDTO Article);
public record UpdateArticleRequest(string Slug, UpdateArticleDTO Article) : IAuthorizationRequest<SingleArticleResponse>;

public class ArticleUpdateValidator : AbstractValidator<UpdateArticleRequest>
{
    public ArticleUpdateValidator()
    {
        RuleFor(x => x.Article.Title).NotEmpty().When(x => x.Article.Title != null);
        RuleFor(x => x.Article.Description).NotEmpty().When(x => x.Article.Description != null);
        RuleFor(x => x.Article.Body).NotEmpty().When(x => x.Article.Body != null);
    }
}

public class ArticleUpdateHandler : IAuthorizationRequestHandler<UpdateArticleRequest, SingleArticleResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public ArticleUpdateHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<SingleArticleResponse> Handle(UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

        if (article.AuthorId != _currentUser.User!.Id)
        {
            throw new ForbiddenException();
        }

        article = _mapper.Map<UpdateArticleDTO, Article>(request.Article, article);

        _context.Articles.Update(article);
        await _context.SaveChangesAsync(cancellationToken);

        return new SingleArticleResponse(_mapper.Map<ArticleDTO>(article));
    }
}
