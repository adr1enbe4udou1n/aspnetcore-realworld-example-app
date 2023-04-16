using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Presentation.Controllers;

[Route("articles/{slug}/[controller]")]
[ApiExplorerSettings(GroupName = "Comments")]
[Authorize]
public class CommentsController
{
    private readonly ISender _sender;

    public CommentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get comments for an article
    /// </summary>
    /// <remarks>Get the comments for an article. Auth is optional</remarks>
    /// <param name="slug">Slug of the article that you want to get comments for</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetArticleComments")]
    [AllowAnonymous]
    public Task<MultipleCommentsResponse> List(string slug, CancellationToken cancellationToken)
    {
        return _sender.Send(new CommentsListQuery(slug), cancellationToken);
    }

    /// <summary>
    /// Create a comment for an article
    /// </summary>
    /// <remarks>Create a comment for an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to create a comment for</param>
    /// <param name="request">Comment you want to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateArticleComment")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<SingleCommentResponse> Create(string slug, [FromBody] NewCommentRequest request, CancellationToken cancellationToken)
    {
        return _sender.Send(new NewCommentCommand(slug, request.Comment), cancellationToken);
    }

    /// <summary>
    /// Delete a comment for an article
    /// </summary>
    /// <remarks>Delete a comment for an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to delete a comment for</param>
    /// <param name="commentId">ID of the comment you want to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{commentId}", Name = "DeleteArticleComment")]
    public Task Delete(string slug, int commentId, CancellationToken cancellationToken)
    {
        return _sender.Send(new CommentDeleteCommand(slug, commentId), cancellationToken);
    }
}

public record NewCommentRequest(NewCommentDto Comment);