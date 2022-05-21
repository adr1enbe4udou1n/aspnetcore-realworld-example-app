using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Commands;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Articles;

public class ArticlesListTests : TestBase
{
    [Test]
    public async Task Can_Paginate_Articles()
    {
        await CreateArticles();

        var response = await Act(new ArticlesListQuery
        {
            Limit = 30,
            Offset = 10
        });

        response.Articles.Count().Should().Be(20);
        response.ArticlesCount.Should().Be(50);

        response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "Jane Doe - Test Title 10",
            Description = "Test Description",
            Body = "Test Body",
            Author = new ProfileDTO
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        }, options => options
            .Excluding(x => x.Slug)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt)
        );
    }

    [Test]
    public async Task Can_Filter_Articles_By_Author()
    {
        await CreateArticles();

        var response = await Act(new ArticlesListQuery
        {
            Limit = 10,
            Offset = 0,
            Author = "John"
        });

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(30);

        response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new ProfileDTO
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        }, options => options
            .Excluding(x => x.Slug)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt)
        );
    }

    [Test]
    public async Task Can_Filter_Articles_By_Tag()
    {
        await CreateArticles();

        var response = await Act(new ArticlesListQuery
        {
            Limit = 10,
            Offset = 0,
            Tag = "Tag Jane Doe",
        });

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(20);

        response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "Jane Doe - Test Title 20",
            Description = "Test Description",
            Body = "Test Body",
            Author = new ProfileDTO
            {
                Username = "Jane Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag Jane Doe" },
        }, options => options
            .Excluding(x => x.Slug)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt)
        );
    }

    [Test]
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
            await _mediator.Send(new ArticleFavoriteRequest(a, true));
        }

        var response = await Act(new ArticlesListQuery
        {
            Limit = 10,
            Offset = 0,
            Favorited = "Jane",
        });

        response.Articles.Count().Should().Be(5);
        response.ArticlesCount.Should().Be(5);

        response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "John Doe - Test Title 16",
            Description = "Test Description",
            Body = "Test Body",
            Author = new ProfileDTO
            {
                Username = "John Doe"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
            Favorited = true,
            FavoritesCount = 1
        }, options => options
            .Excluding(x => x.Slug)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt)
        );
    }

    [Test]
    public async Task Guest_Cannot_Paginate_Articles_Of_Followed_Authors()
    {
        await this.Invoking(x => x.Act(new ArticlesFeedQuery()))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Can_Paginate_Articles_Of_Followed_Authors()
    {
        await CreateArticles();

        await _mediator.Send(new ProfileFollowRequest("John Doe", true));

        var response = await Act(new ArticlesFeedQuery
        {
            Limit = 10,
            Offset = 0
        });

        response.Articles.Count().Should().Be(10);
        response.ArticlesCount.Should().Be(30);

        response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "John Doe - Test Title 30",
            Description = "Test Description",
            Body = "Test Body",
            Author = new ProfileDTO
            {
                Username = "John Doe",
                Following = true
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Tag John Doe" },
        }, options => options
            .Excluding(x => x.Slug)
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt)
        );
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

        for (int i = 1; i <= count; i++)
        {
            articles.Add($"{author.Name} - Test Title {i}");
        }

        foreach (var a in articles)
        {
            await _mediator.Send(new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = a,
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2", $"Tag {author.Name}" }
                }
            ));
        }
    }
}