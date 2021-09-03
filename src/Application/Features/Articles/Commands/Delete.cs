using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Articles.Commands
{
    public record ArticleDeleteCommand(string Slug) : IAuthorizationRequest;

    public class ArticleDeleteHandler : IAuthorizationRequestHandler<ArticleDeleteCommand>
    {
        private readonly IAppDbContext _context;

        public ArticleDeleteHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<Unit> Handle(ArticleDeleteCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}