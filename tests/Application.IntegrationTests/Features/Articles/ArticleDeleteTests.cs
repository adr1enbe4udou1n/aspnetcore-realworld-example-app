using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Articles;

public class ArticleDeleteTests : TestBase
{
    [Test]
    public async Task Guest_Cannot_Delete_Article()
    {
        var response = await Act(HttpMethod.Delete, "/articles/slug-article");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Delete, "/articles/slug-article");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Cannot_Delete_Article_Of_Other_Author()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
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

        var response = await Act(HttpMethod.Delete, "/articles/test-title");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Can_Delete_Own_Article_With_All_Comments()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        for (int i = 1; i <= 5; i++)
        {
            await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = $"This is John, Test Comment {i} !",
            }));
        }

        await _mediator.Send(new ArticleFavoriteRequest("test-title", true));

        await Act(HttpMethod.Delete, "/articles/test-title");

        (await _context.Articles.AnyAsync()).Should().BeFalse();
        (await _context.Comments.AnyAsync()).Should().BeFalse();
    }
}