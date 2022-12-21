using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleFavoriteTests : TestBase
{
    public ArticleFavoriteTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Guest_Cannot_Favorite_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles/slug-article/favorite");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
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

    [Fact]
    public async Task Can_Favorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Post, "/articles/test-title/favorite");

        response.Article.Should().BeEquivalentTo(new ArticleDto
        {
            Favorited = true,
            FavoritesCount = 1,
        }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

        (await Context.Set<ArticleFavorite>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Can_Unfavorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await Mediator.Send(new ArticleFavoriteRequest("test-title", true));

        var response = await Act<SingleArticleResponse>(HttpMethod.Delete, "/articles/test-title/favorite");

        response.Article.Should().BeEquivalentTo(new ArticleDto
        {
            Favorited = false,
            FavoritesCount = 0,
        }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

        (await Context.Set<ArticleFavorite>().CountAsync()).Should().Be(0);
    }
}