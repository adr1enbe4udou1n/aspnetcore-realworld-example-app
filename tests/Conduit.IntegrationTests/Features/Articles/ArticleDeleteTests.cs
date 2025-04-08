using System.Net;

using Conduit.Domain.Entities;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticleDeleteTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Guest_Cannot_Delete_Article()
    {
        var response = await Act(HttpMethod.Delete, "/articles/slug-article");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Delete, "/articles/slug-article");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Article_Of_Other_Author()
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

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await Act(HttpMethod.Delete, "/articles/test-title");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Can_Delete_Own_Article_With_All_Comments()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var article = new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        };

        Context.Articles.Add(article);

        await Context.SaveChangesAsync();

        for (var i = 1; i <= 5; i++)
        {
            var comment = new Comment
            {
                Body = $"This is John, Test Comment {i} !",
                Article = article,
                Author = user,
            };

            Context.Comments.Add(comment);
        }

        await Act(HttpMethod.Delete, "/articles/test-title");

        Assert.False(await Context.Articles.AnyAsync());
        Assert.False(await Context.Comments.AnyAsync());
    }
}