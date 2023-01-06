using System.Net;
using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Features.Profiles.Queries;
using Conduit.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticleUpdateTests : TestBase
{
    public ArticleUpdateTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidArticles()
    {
        yield return new object[]
        {
            new UpdateArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            }
        };
    }

    [Theory, MemberData(nameof(InvalidArticles))]
    public async Task Cannot_Update_Article_With_Invalid_Data(UpdateArticleDto article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Put, "/articles/test-title", new UpdateArticleRequest(article));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Guest_Cannot_Update_Article()
    {
        var response = await Act(HttpMethod.Put, "/articles/slug-article", new UpdateArticleRequest(
            new UpdateArticleDto()
        ));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cannot_Update_Article_Of_Other_Author()
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Can_Update_Own_Article()
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

        var response = await Act<SingleArticleResponse>(HttpMethod.Put, "/articles/test-title",
            new UpdateArticleRequest(
                new UpdateArticleDto
                {
                    Title = "New Title",
                    Description = "New Description",
                }
            )
        );

        response.Article.Should().BeEquivalentTo(new ArticleDto
        {
            Title = "New Title",
            Description = "New Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new ProfileDto
            {
                Username = "John Doe",
            },
            TagList = new List<string>(),
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await Context.Articles.AnyAsync(x => x.Title == "New Title")).Should().BeTrue();
    }
}