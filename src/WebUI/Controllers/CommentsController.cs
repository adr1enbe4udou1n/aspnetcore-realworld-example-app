using System.Threading;
using System.Threading.Tasks;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("articles/{slug}/[controller]")]
    [ApiExplorerSettings(GroupName = "Comments")]
    public class CommentsController
    {
        private readonly IMediator _mediator;

        public CommentsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<CommentsEnvelope> List(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentsListQuery(slug), cancellationToken);

        [HttpPost]
        [Authorize]
        public async Task<CommentEnvelope> Create(string slug, [FromBody] CommentCreateBody command, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentCreateCommand(slug, command.Comment), cancellationToken);

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task Delete(string slug, int commentId, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentDeleteCommand(slug, commentId), cancellationToken);
    }
}
