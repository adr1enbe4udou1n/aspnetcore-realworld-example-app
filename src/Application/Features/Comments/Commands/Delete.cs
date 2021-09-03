using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Comments.Queries;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Comments.Commands
{
    public record CommentDeleteCommand(string slug, int id) : IAuthorizationRequest;

    public class CommentDeleteHandler : IAuthorizationRequestHandler<CommentDeleteCommand>
    {
        private readonly IAppDbContext _context;

        public CommentDeleteHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<Unit> Handle(CommentDeleteCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}