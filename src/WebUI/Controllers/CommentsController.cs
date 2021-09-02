using System.Threading;
using System.Threading.Tasks;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("articles")]
    public class CommentsController
    {
        private readonly IMediator _mediator;

        public CommentsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{slug}/comments")]
        public async Task<CommentsEnvelope> List(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentsListQuery(slug), cancellationToken);

        [HttpPost("{slug}/comments")]
        public async Task<CommentEnvelope> Create(string slug, [FromBody] CommentCreateDTO comment, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentCreateCommand(slug, comment), cancellationToken);

        [HttpDelete("{slug}/comments/{commentId}")]
        public async Task Delete(string slug, int commentId, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentDeleteCommand(slug, commentId), cancellationToken);
    }
}