namespace Conduit.Application.Features.Tags.Queries;

public interface IQueryTags
{
    Task<TagsResponse> List(CancellationToken cancellationToken);
}