using System.Net;

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
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        Context.Articles.Add(new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        });

        await Context.SaveChangesAsync();

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
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        Context.Articles.Add(new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        });

        await Context.SaveChangesAsync();

        Context.ArticleFavorite.Add(new ArticleFavorite
        {
            Article = await Context.Articles.FirstAsync(x => x.Slug == "test-title"),
            User = user,
        });

        var response = await Act<SingleArticleResponse>(HttpMethod.Delete, "/articles/test-title/favorite");

        Assert.Equivalent(new
        {
            Favorited = false,
            FavoritesCount = 0,
        }, response.Article);
        Assert.Equal(0, await Context.Set<ArticleFavorite>().CountAsync());
    }
}