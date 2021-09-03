using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Articles.Commands
{
    public record ArticleFavoriteCommand(string Slug, bool Favorite) : IAuthorizationRequest<ArticleEnvelope>;

    public class ArticleFavoriteHandler : IAuthorizationRequestHandler<ArticleFavoriteCommand, ArticleEnvelope>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ArticleFavoriteHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ArticleEnvelope> Handle(ArticleFavoriteCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.Articles.FindAsync(x => x.Slug == request.Slug, cancellationToken);

            if (request.Favorite)
            {
                if (!_currentUser.User.IsFavorite(article))
                {
                    _currentUser.User.FavoriteArticles.Add(new ArticleFavorite { Article = article });
                }
            }
            else
            {
                if (_currentUser.User.IsFavorite(article))
                {
                    _currentUser.User.FavoriteArticles.RemoveAll(x => x.ArticleId == article.Id);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ArticleEnvelope(_mapper.Map<ArticleDTO>(article));
        }
    }
}