using System.Collections.ObjectModel;
using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

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

public class ArticleCreateTests : TestBase
{
    public ArticleCreateTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Theory, ClassData(typeof(InvalidNewArticles))]
    public async Task Cannot_Create_Article_With_Invalid_Data(NewArticleDto article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Act(HttpMethod.Post, "/articles", new NewArticleCommand(article));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Guest_Cannot_Create_Article()
    {
        var response = await Act(HttpMethod.Post, "/articles", new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
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

        await Context.Tags.AddAsync(new Tag
        {
            Name = "Existing Tag"
        });
        await Context.SaveChangesAsync();

        var response = await Act<SingleArticleResponse>(
            HttpMethod.Post, "/articles",
            new NewArticleCommand(
                new NewArticleDto
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new Collection<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" },
                }
            )
        );

        response.Article.Should().BeEquivalentTo(new
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
        });

        (await Context.Articles.AnyAsync()).Should().BeTrue();
        (await Context.Tags.CountAsync()).Should().Be(3);
    }
}