using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Comments.Queries;
using Application.Interfaces;

namespace Application.Features.Comments.Commands
{
    public class CommentCreateDTO
    {

        public string Body { get; set; }
    }

    public record CommentEnvelope(CommentDTO Comment);

    public record CommentCreateCommand(string Slug, CommentCreateDTO Comment) : IAuthorizationRequest<CommentEnvelope>;

    public class CommentCreateHandler : IAuthorizationRequestHandler<CommentCreateCommand, CommentEnvelope>
    {
        private readonly IAppDbContext _context;

        public CommentCreateHandler(IAppDbContext context)
        {
            _context = context;
        }

        public Task<CommentEnvelope> Handle(CommentCreateCommand request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}