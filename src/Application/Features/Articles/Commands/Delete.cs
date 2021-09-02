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
    public record ArticleDeleteCommand(string slug) : IAuthorizationRequest;

    public class ArticleDeleteHandler : IAuthorizationRequestHandler<ArticleDeleteCommand>
    {
        public Task<Unit> Handle(ArticleDeleteCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}