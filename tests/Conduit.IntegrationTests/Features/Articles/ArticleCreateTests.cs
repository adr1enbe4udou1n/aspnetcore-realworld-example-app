using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Articles.Queries;
using Conduit.Application.Features.Auth.Commands;
using Conduit.Application.Features.Auth.Queries;
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
    }
}

public class ArticleCreateTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{
    private readonly ConduitApiFixture _factory = factory;

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

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
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
            ),
            HttpStatusCode.Created
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

        Assert.Equal(0, response.Article.CreatedAt.Ticks % 10);
        Assert.Equal(0, response.Article.UpdatedAt.Ticks % 10);
        Assert.True(await Context.Articles.AnyAsync());
        Assert.Equal(3, await Context.Tags.CountAsync());
    }

    [Fact]
    public async Task Can_Create_Articles_With_Duplicate_Titles()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com"
        });

        var request = new NewArticleRequest(new NewArticleDto
        {
            Title = "Duplicate Title",
            Description = "Test Description",
            Body = "Test Body"
        });

        var first = await Act<SingleArticleResponse>(HttpMethod.Post, "/articles", request);
        var second = await Act<SingleArticleResponse>(HttpMethod.Post, "/articles", request);

        Assert.NotEqual(first.Article.Slug, second.Article.Slug);
    }

    [Fact]
    public async Task Can_Create_Articles_With_The_Same_New_Tag_Concurrently()
    {
        var suffix = Guid.NewGuid().ToString("N");
        using var firstClient = await CreateAuthenticatedClient($"first-{suffix}");
        using var secondClient = await CreateAuthenticatedClient($"second-{suffix}");
        var sharedTag = $"shared-{suffix}";

        var responses = await Task.WhenAll(
            CreateArticle(firstClient, $"First {suffix}", sharedTag),
            CreateArticle(secondClient, $"Second {suffix}", sharedTag));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(1, await Context.Tags.CountAsync(tag => tag.Name == sharedTag));
    }

    private async Task<HttpClient> CreateAuthenticatedClient(string username)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/users", new NewUserRequest(new NewUserDto
        {
            Username = username,
            Email = $"{username}@test.com",
            Password = "password123"
        }));
        response.EnsureSuccessStatusCode();
        var user = (await response.Content.ReadFromJsonAsync<UserResponse>())!.User;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", user.Token);
        return client;
    }

    private static Task<HttpResponseMessage> CreateArticle(HttpClient client, string title, string tag)
    {
        return client.PostAsJsonAsync("/api/articles", new NewArticleRequest(new NewArticleDto
        {
            Title = title,
            Description = "Test Description",
            Body = "Test Body",
            TagList = [tag]
        }));
    }
}