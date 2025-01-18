using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticleFavoriteTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Guest_Cannot_Favorite_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles/slug-article/favorite");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Can_Favorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Post, "/articles/test-title/favorite");

        Assert.Equivalent(new
        {
            Favorited = true,
            FavoritesCount = 1,
        }, response.Article);
        Assert.Equal(1, await Context.Set<ArticleFavorite>().CountAsync());
    }

    [Fact]
    public async Task Can_Unfavorite_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await Mediator.Send(new ArticleFavoriteCommand("test-title", true));

        var response = await Act<SingleArticleResponse>(HttpMethod.Delete, "/articles/test-title/favorite");

        Assert.Equivalent(new
        {
            Favorited = false,
            FavoritesCount = 0,
        }, response.Article);
        Assert.Equal(0, await Context.Set<ArticleFavorite>().CountAsync());
    }
}