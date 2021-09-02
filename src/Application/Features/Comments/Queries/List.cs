using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Support;
using MediatR;

namespace Application.Features.Comments.Queries
{
    public class CommentDTO
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public AuthorDTO Author { get; set; }
    }

    public class CommentsEnvelope
    {
        public IEnumerable<CommentDTO> Comments { get; set; }
    }

    public record CommentsListQuery(string slug) : IRequest<CommentsEnvelope>;

    public class CommentsListHandler : IRequestHandler<CommentsListQuery, CommentsEnvelope>
    {
        public Task<CommentsEnvelope> Handle(CommentsListQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}