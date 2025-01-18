using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Comments.Commands;
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

        for (var i = 1; i <= 5; i++)
        {
            await Mediator.Send(new NewCommentCommand("test-title", new NewCommentDto
            {
                Body = $"This is John, Test Comment {i} !",
            }));
        }

        await Mediator.Send(new ArticleFavoriteCommand("test-title", true));

        await Act(HttpMethod.Delete, "/articles/test-title");

        Assert.False(await Context.Articles.AnyAsync());
        Assert.False(await Context.Comments.AnyAsync());
    }
}