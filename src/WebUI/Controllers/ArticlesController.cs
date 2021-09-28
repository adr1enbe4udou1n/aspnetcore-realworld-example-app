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

        /// <summary>
        /// Get recent articles globally
        /// </summary>
        /// <remarks>Get most recent articles globally. Use query parameters to filter results. Auth is optional</remarks>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ArticlesEnvelope> List([FromQuery] ArticlesListQuery query, CancellationToken cancellationToken)
            => await _mediator.Send(query, cancellationToken);

        /// <summary>
        /// Get recent articles from users you follow
        /// </summary>
        /// <remarks>Get most recent articles from users you follow. Use query parameters to limit. Auth is required</remarks>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("feed")]
        [Authorize]
        public async Task<ArticlesEnvelope> Feed([FromQuery] ArticlesFeedQuery query, CancellationToken cancellationToken)
            => await _mediator.Send(query, cancellationToken);

        /// <summary>
        /// Get an article
        /// </summary>
        /// <remarks>Get an article. Auth not required</remarks>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{slug}")]
        public async Task<ArticleEnvelope> Get(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleGetQuery(slug), cancellationToken);

        /// <summary>
        /// Create an article
        /// </summary>
        /// <remarks>Create an article. Auth is required</remarks>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ArticleEnvelope> Create([FromBody] ArticleCreateCommand command, CancellationToken cancellationToken)
            => await _mediator.Send(command, cancellationToken);

        /// <summary>
        /// Update an article
        /// </summary>
        /// <remarks>Update an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{slug}")]
        [Authorize]
        public async Task<ArticleEnvelope> Update(string slug, [FromBody] ArticleUpdateBody command, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleUpdateCommand(slug, command.Article), cancellationToken);

        /// <summary>
        /// Delete an article
        /// </summary>
        /// <remarks>Delete an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{slug}")]
        [Authorize]
        public async Task Delete(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleDeleteCommand(slug), cancellationToken);

        /// <summary>
        /// Favorite an article
        /// </summary>
        /// <remarks>Favorite an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{slug}/favorite")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "Favorites")]
        public async Task<ArticleEnvelope> Favorite(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleFavoriteCommand(slug, true), cancellationToken);

        /// <summary>
        /// Unfavorite an article
        /// </summary>
        /// <remarks>Unfavorite an article. Auth is required</remarks>
        /// <param name="slug"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{slug}/favorite")]
        [Authorize]
        [ApiExplorerSettings(GroupName = "Favorites")]
        public async Task<ArticleEnvelope> Unfavorite(string slug, CancellationToken cancellationToken)
            => await _mediator.Send(new ArticleFavoriteCommand(slug, false), cancellationToken);
    }
}
