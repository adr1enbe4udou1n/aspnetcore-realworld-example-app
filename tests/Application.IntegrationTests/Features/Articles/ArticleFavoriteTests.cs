using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleFavoriteTests : TestBase
{
    [Test]
    public async Task Guest_Cannot_Favorite_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles/slug-article/favorite");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Cannot_Favorite_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Post, "/articles/slug-article/favorite");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Can_Favorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Post, "/articles/test-title/favorite");

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Favorited = true,
            FavoritesCount = 1,
        }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

        (await _context.Set<ArticleFavorite>().CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task Can_Unfavorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await _mediator.Send(new ArticleFavoriteRequest("test-title", true));

        var response = await Act<SingleArticleResponse>(HttpMethod.Delete, "/articles/test-title/favorite");

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Favorited = false,
            FavoritesCount = 0,
        }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

        (await _context.Set<ArticleFavorite>().CountAsync()).Should().Be(0);
    }
}