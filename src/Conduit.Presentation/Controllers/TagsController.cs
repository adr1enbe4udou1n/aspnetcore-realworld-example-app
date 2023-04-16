using Conduit.Application.Features.Tags.Queries;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace Conduit.Presentation.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Tags")]
public class TagsController
{
    private readonly ISender _sender;

    public TagsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get tags
    /// </summary>
    /// <remarks>Get tags. Auth not required</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetTags")]
    public Task<TagsResponse> List(CancellationToken cancellationToken)
    {
        return _sender.Send(new TagsListQuery(), cancellationToken);
    }
}