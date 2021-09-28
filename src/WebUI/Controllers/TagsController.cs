using System.Threading;
using System.Threading.Tasks;
using Application.Features.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "Tags")]
    public class TagsController
    {
        private readonly IMediator _mediator;

        public TagsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<TagsEnvelope> List(CancellationToken cancellationToken)
            => await _mediator.Send(new TagsListQuery(), cancellationToken);
    }
}
