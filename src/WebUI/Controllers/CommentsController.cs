using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

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
    /// <param name="slug">Slug of the article that you want to get comments for</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<MultipleCommentsResponse> List(string slug, CancellationToken cancellationToken)
        => await _mediator.Send(new CommentsListQuery(slug), cancellationToken);

    /// <summary>
    /// Create a comment for an article
    /// </summary>
    /// <remarks>Create a comment for an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to create a comment for</param>
    /// <param name="command">Comment you want to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<SingleCommentResponse> Create(string slug, [FromBody] NewCommentBody command, CancellationToken cancellationToken)
        => await _mediator.Send(new NewCommentRequest(slug, command.Comment), cancellationToken);

    /// <summary>
    /// Delete a comment for an article
    /// </summary>
    /// <remarks>Delete a comment for an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to delete a comment for</param>
    /// <param name="commentId">ID of the comment you want to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task Delete(string slug, int commentId, CancellationToken cancellationToken)
        => await _mediator.Send(new CommentDeleteRequest(slug, commentId), cancellationToken);
}
