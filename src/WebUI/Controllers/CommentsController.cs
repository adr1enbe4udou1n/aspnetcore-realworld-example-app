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

        /// <summary>
        /// Get comments for an article
        /// </summary>
        /// <remarks>Get the comments for an article. Auth is optional</remarks>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<CommentsEnvelope> List(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentsListQuery(slug), cancellationToken);

        /// <summary>
        /// Create a comment for an article
        /// </summary>
        /// <remarks>Create a comment for an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<CommentEnvelope> Create(string slug, [FromBody] CommentCreateBody command, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentCreateCommand(slug, command.Comment), cancellationToken);

        /// <summary>
        /// Delete a comment for an article
        /// </summary>
        /// <remarks>Delete a comment for an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="commentId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task Delete(string slug, int commentId, CancellationToken cancellationToken)
            => await _mediator.Send(new CommentDeleteCommand(slug, commentId), cancellationToken);
    }
}
