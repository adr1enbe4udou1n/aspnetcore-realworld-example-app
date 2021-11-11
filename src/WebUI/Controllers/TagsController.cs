using Application.Features.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Tags")]
public class TagsController
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get tags
    /// </summary>
    /// <remarks>Get tags. Auth not required</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<TagsResponse> List(CancellationToken cancellationToken)
        => await _mediator.Send(new TagsListQuery(), cancellationToken);
}