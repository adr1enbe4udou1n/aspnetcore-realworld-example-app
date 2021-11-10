using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Interfaces;
using Application.Support;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Articles.Queries;

public class ArticlesFeedQuery : PagedQuery, IAuthorizationRequest<MultipleArticlesResponse>
{
}

public class ArticlesFeedHandler : IAuthorizationRequestHandler<ArticlesFeedQuery, MultipleArticlesResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public ArticlesFeedHandler(IAppDbContext context, IMapper mapper, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<MultipleArticlesResponse> Handle(ArticlesFeedQuery request, CancellationToken cancellationToken)
    {
        var articles = await _context.Articles
            .HasAuthorsFollowedBy(_currentUser.User!)
            .OrderByDescending(x => x.Id)
            .ProjectTo<ArticleDTO>(_mapper.ConfigurationProvider, new
            {
                currentUser = _currentUser.User
            })
            .PaginateAsync(request, cancellationToken);

        return new MultipleArticlesResponse(articles.Items, articles.Total);
    }
}
