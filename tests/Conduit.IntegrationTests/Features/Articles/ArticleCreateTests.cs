using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;
using Conduit.Presentation.Endpoints;

using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class InvalidNewArticles : TheoryData<NewArticleDto>
{
    public InvalidNewArticles()
    {
        Add(new NewArticleDto
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "",
        });
        Add(new NewArticleDto
        {
            Title = "Test Title",
            Description = "",
            Body = "Test Body",
        });
        Add(new NewArticleDto
        {
            Title = "",
            Description = "Test Description",
            Body = "Test Body",
        });
        Add(new NewArticleDto
        {
            Title = "Existing Title",
            Description = "Test Description",
            Body = "Test Body",
        });
    }
}

public class ArticleCreateTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Theory, ClassData(typeof(InvalidNewArticles))]
    public async Task Cannot_Create_Article_With_Invalid_Data(NewArticleDto article)
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        Context.Articles.Add(new Article
        {
            Title = "Existing Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "existing-title",
            Author = user,
        });

        await Context.SaveChangesAsync();

        var response = await Act(HttpMethod.Post, "/articles", new NewArticleRequest(article));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Guest_Cannot_Create_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles", new NewArticleRequest(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Can_Create_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Context.Tags.AddAsync(new Tag
        {
            Name = "Existing Tag"
        });
        await Context.SaveChangesAsync();

        var response = await Act<SingleArticleResponse>(
            HttpMethod.Post, "/articles",
            new NewArticleRequest(
                new NewArticleDto
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = ["Test Tag 1", "Test Tag 2", "Existing Tag"],
                }
            )
        );

        Assert.Equivalent(new
        {
            Title = "Test Article",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-article",
            Author = new
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
            TagList = new Collection<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" },
        }, response.Article);

        Assert.True(await Context.Articles.AnyAsync());
        Assert.Equal(3, await Context.Tags.CountAsync());
    }
}