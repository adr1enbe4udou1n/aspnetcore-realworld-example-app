using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;
using Conduit.Presentation.Endpoints;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class InvalidUpdateArticles : TheoryData<UpdateArticleDto>
{
    public InvalidUpdateArticles()
    {
        Add(new UpdateArticleDto
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "",
        });
    }
}

public class ArticleUpdateTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Theory, ClassData(typeof(InvalidUpdateArticles))]
    public async Task Cannot_Update_Article_With_Invalid_Data(UpdateArticleDto article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/articles/test-title", new UpdateArticleRequest(article));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Update_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/articles/slug-article", new UpdateArticleRequest(
            new UpdateArticleDto
            {
                Title = "New Title",
                Description = "New Description",
                Body = "New Body",
            }
        ));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Guest_Cannot_Update_Article()
    {
        var response = await Act(HttpMethod.Put, "/articles/slug-article", new UpdateArticleRequest(
            new UpdateArticleDto()
        ));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Update_Article_Of_Other_Author()
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

        var response = await Act(
            HttpMethod.Put, "/articles/test-title",
            new UpdateArticleRequest(
                new UpdateArticleDto
                {
                    Title = "New Title",
                    Description = "New Description",
                    Body = "New Body",
                }
            ));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Can_Update_Own_Article()
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

        var response = await Act<SingleArticleResponse>(HttpMethod.Put, "/articles/test-title",
            new UpdateArticleRequest(
                new UpdateArticleDto
                {
                    Title = "New Title",
                    Description = "New Description",
                }
            )
        );

        Assert.Equivalent(new
        {
            Title = "New Title",
            Description = "New Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new
            {
                Username = "John Doe",
            },
            TagList = new List<string>(),
        }, response.Article);

        Assert.True(await Context.Articles.AnyAsync(x => x.Title == "New Title"));
    }
}