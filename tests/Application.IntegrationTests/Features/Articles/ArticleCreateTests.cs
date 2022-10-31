using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleCreateTests : TestBase
{
    public ArticleCreateTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> InvalidArticles()
    {
        yield return new object[]
        {
            new NewArticleDTO
            {
                Title = "",
                Description = "Test Description",
                Body = "Test Body",
            },
        };
        yield return new object[]
        {
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "",
                Body = "Test Body",
            },
        };
        yield return new object[]
        {
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            },
        };
        yield return new object[]
        {
            new NewArticleDTO
            {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            },
        };
    }

    [Theory, MemberData(nameof(InvalidArticles))]
    public async Task Cannot_Create_Article_With_Invalid_Data(NewArticleDTO article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act(HttpMethod.Post, "/articles", new NewArticleRequest(article));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Guest_Cannot_Create_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles", new NewArticleRequest(
            new NewArticleDTO()
        ));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

        await _context.Tags.AddAsync(new Tag
        {
            Name = "Existing Tag"
        });
        await _context.SaveChangesAsync();

        var response = await Act<SingleArticleResponse>(
            HttpMethod.Post, "/articles",
            new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" }
                }
            )
        );

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "Test Article",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-article",
            Author = new ProfileDTO
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" },
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await _context.Articles.AnyAsync()).Should().BeTrue();
        (await _context.Tags.CountAsync()).Should().Be(3);
    }
}