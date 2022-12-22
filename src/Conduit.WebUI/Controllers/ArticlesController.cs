using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Articles")]
[Authorize]
public class ArticlesController
{
    private readonly ISender _sender;

    public ArticlesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get recent articles globally
    /// </summary>
    /// <remarks>Get most recent articles globally. Use query parameters to filter results. Auth is optional</remarks>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetArticles")]
    [AllowAnonymous]
    public Task<MultipleArticlesResponse> List([FromQuery] ArticlesListQuery query, CancellationToken cancellationToken)
    {
        return _sender.Send(query, cancellationToken);
    }

    /// <summary>
    /// Get recent articles from users you follow
    /// </summary>
    /// <remarks>Get most recent articles from users you follow. Use query parameters to limit. Auth is required</remarks>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("feed", Name = "GetArticlesFeed")]
    public Task<MultipleArticlesResponse> Feed([FromQuery] ArticlesFeedQuery query, CancellationToken cancellationToken)
    {
        return _sender.Send(query, cancellationToken);
    }

    /// <summary>
    /// Get an article
    /// </summary>
    /// <remarks>Get an article. Auth not required</remarks>
    /// <param name="slug">Slug of the article to get</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{slug}", Name = "GetArticle")]
    [AllowAnonymous]
    public Task<SingleArticleResponse> Get(string slug, CancellationToken cancellationToken)
    {
        return _sender.Send(new ArticleGetQuery(slug), cancellationToken);
    }

    /// <summary>
    /// Create an article
    /// </summary>
    /// <remarks>Create an article. Auth is required</remarks>
    /// <param name="command">Article to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateArticle")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<SingleArticleResponse> Create([FromBody] NewArticleRequest command, CancellationToken cancellationToken)
    {
        return _sender.Send(command, cancellationToken);
    }

    /// <summary>
    /// Update an article
    /// </summary>
    /// <remarks>Update an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article to update</param>
    /// <param name="command">Article to update</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{slug}", Name = "UpdateArticle")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public Task<SingleArticleResponse> Update(string slug, [FromBody] UpdateArticleBody command, CancellationToken cancellationToken)
    {
        return _sender.Send(new UpdateArticleRequest(slug, command.Article), cancellationToken);
    }

    /// <summary>
    /// Delete an article
    /// </summary>
    /// <remarks>Delete an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{slug}", Name = "DeleteArticle")]
    public Task Delete(string slug, CancellationToken cancellationToken)
    {
        return _sender.Send(new ArticleDeleteRequest(slug), cancellationToken);
    }

    /// <summary>
    /// Favorite an article
    /// </summary>
    /// <remarks>Favorite an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to favorite</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{slug}/favorite", Name = "CreateArticleFavorite")]
    [ApiExplorerSettings(GroupName = "Favorites")]
    public Task<SingleArticleResponse> Favorite(string slug, CancellationToken cancellationToken)
    {
        return _sender.Send(new ArticleFavoriteRequest(slug, true), cancellationToken);
    }

    /// <summary>
    /// Unfavorite an article
    /// </summary>
    /// <remarks>Unfavorite an article. Auth is required</remarks>
    /// <param name="slug">Slug of the article that you want to unfavorite</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{slug}/favorite", Name = "DeleteArticleFavorite")]
    [ApiExplorerSettings(GroupName = "Favorites")]
    public Task<SingleArticleResponse> Unfavorite(string slug, CancellationToken cancellationToken)
    {
        return _sender.Send(new ArticleFavoriteRequest(slug, false), cancellationToken);
    }
}