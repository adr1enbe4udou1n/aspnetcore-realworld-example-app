using System.Collections.ObjectModel;
using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Features.Profiles.Commands;
using Conduit.Domain.Entities;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Articles;

public class ArticlesListTests : TestBase
{
    public ArticlesListTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Can_Paginate_Articles()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=30&offset=10");

        response.Articles.Count().Should().Be(20);
        response.ArticlesCount.Should().Be(50);

        response.Articles.First().Should().BeEquivalentTo(new
        {
            Title = "Jane Doe - Test Title 10",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        });
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Author()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&author=John Doe");

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(30);

        response.Articles.First().Should().BeEquivalentTo(new
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        });
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Tag()
    {
        await CreateArticles();

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&tag=Tag Jane Doe");

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(20);

        response.Articles.First().Should().BeEquivalentTo(new
        {
            Title = "Jane Doe - Test Title 20",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        });
    }

    [Fact]
    public async Task Can_Filter_Articles_By_Favorited()
    {
        await CreateArticles();

        var articles = new List<string>
            {
                "john-doe-test-title-1",
                "john-doe-test-title-2",
                "john-doe-test-title-4",
                "john-doe-test-title-8",
                "john-doe-test-title-16",
            };

        foreach (var a in articles)
        {
            await Mediator.Send(new ArticleFavoriteCommand(a, true));
        }

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles?limit=10&offset=0&favorited=Jane Doe");

        response.Articles.Count().Should().Be(5);
        response.ArticlesCount.Should().Be(5);

        response.Articles.First().Should().BeEquivalentTo(new
        {
            Title = "John Doe - Test Title 16",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
            Favorited = true,
            FavoritesCount = 1
        });
    }

    [Fact]
    public async Task Guest_Cannot_Paginate_Articles_Of_Followed_Authors()
    {
        var response = await Act(HttpMethod.Get, "/articles/feed");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Can_Paginate_Articles_Of_Followed_Authors()
    {
        await CreateArticles();

        await Mediator.Send(new ProfileFollowCommand("John Doe", true));

        var response = await Act<MultipleArticlesResponse>(HttpMethod.Get, "/articles/feed?limit=10&offset=0");

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(30);

        response.Articles.First().Should().BeEquivalentTo(new
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new
            {
                Username = "John Doe",
                Following = true
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        });
    }

    private async Task CreateArticles()
    {
        await CreateArticlesForAuthor(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        }, 30);

        await CreateArticlesForAuthor(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        }, 20);
    }

    private async Task CreateArticlesForAuthor(User author, int count)
    {
        await ActingAs(author);

        var articles = new List<string>();

        for (var i = 1; i <= count; i++)
        {
            articles.Add($"{author.Name} - Test Title {i}");
        }

        foreach (var a in articles)
        {
            await Mediator.Send(new NewArticleCommand(
                new NewArticleDto
                {
                    Title = a,
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new Collection<string> { "Test Tag 1", "Test Tag 2", $"Tag {author.Name}" }
                }
            ));
        }
    }
}