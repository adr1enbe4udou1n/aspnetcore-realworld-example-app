namespace Conduit.Application.Features.Comments.Queries;

public interface IQueryComments
{
    Task<MultipleCommentsResponse> List(string slug, CancellationToken cancellationToken);
}