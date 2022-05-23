using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleGetTests : TestBase
{
    [Test]
    public async Task Cannot_Get_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Get, "/articles/slug-article");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Can_Get_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
                TagList = new List<string> { "Test Tag 1", "Test Tag 2" }
            }
        ));

        var response = await Act<SingleArticleResponse>(HttpMethod.Get, "/articles/test-title");

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new ProfileDTO
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
            TagList = new List<string> { "Test Tag 1", "Test Tag 2" },
            Favorited = false,
            FavoritesCount = 0,
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));
    }
}