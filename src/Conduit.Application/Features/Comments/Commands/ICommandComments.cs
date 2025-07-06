namespace Conduit.Application.Features.Comments.Commands;

public interface ICommandComments
{
    Task<SingleCommentResponse> Create(string slug, NewCommentDto newComment, CancellationToken cancellationToken);
    Task Delete(string slug, int id, CancellationToken cancellationToken);
}