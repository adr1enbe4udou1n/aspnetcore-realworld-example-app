using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;


using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticleGetTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Cannot_Get_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Get, "/articles/slug-article");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Can_Get_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
                TagList = ["Test Tag 1", "Test Tag 2"]
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Get, "/articles/test-title");

        Assert.Equivalent(new
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2" },
            Favorited = false,
            FavoritesCount = 0,
        }, response.Article);
    }
}