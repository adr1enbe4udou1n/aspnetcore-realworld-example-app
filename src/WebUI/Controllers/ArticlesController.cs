using System.Threading;
using System.Threading.Tasks;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "Articles")]
    public class ArticlesController
    {
        private readonly IMediator _mediator;

        public ArticlesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ArticlesEnvelope> List([FromQuery] ArticlesListQuery query, CancellationToken cancellationToken)
            => await _mediator.Send(query, cancellationToken);

        [HttpGet("feed")]
        [Authorize]
        public async Task<ArticlesEnvelope> Feed([FromQuery] ArticlesFeedQuery query, CancellationToken cancellationToken)
            => await _mediator.Send(query, cancellationToken);

        [HttpGet("{slug}")]
        public async Task<ArticleEnvelope> Get(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleGetQuery(slug), cancellationToken);

        [HttpPost]
        [Authorize]
        public async Task<ArticleEnvelope> Create([FromBody] ArticleCreateCommand command, CancellationToken cancellationToken)
            => await _mediator.Send(command, cancellationToken);

        [HttpPut("{slug}")]
        [Authorize]
        public async Task<ArticleEnvelope> Update(string slug, [FromBody] ArticleUpdateBody command, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleUpdateCommand(slug, command.Article), cancellationToken);

        [HttpDelete("{slug}")]
        [Authorize]
        public async Task Delete(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleDeleteCommand(slug), cancellationToken);

        [HttpPost("{slug}/favorite")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "Favorites")]
        public async Task<ArticleEnvelope> Favorite(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleFavoriteCommand(slug, true), cancellationToken);

        [HttpDelete("{slug}/favorite")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "Favorites")]
        public async Task<ArticleEnvelope> Unfavorite(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleFavoriteCommand(slug, false), cancellationToken);
    }
}
