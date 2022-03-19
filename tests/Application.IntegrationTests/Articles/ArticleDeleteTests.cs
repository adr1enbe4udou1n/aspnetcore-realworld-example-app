using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Articles;

public class ArticleDeleteTests : TestBase
{
    [Test]
    public async Task Guest_Cannot_Delete_Article()
    {
        await this.Invoking(x => x.Act(new ArticleDeleteRequest("slug-article")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Cannot_Delete_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await this.Invoking(x => x.Act(new ArticleDeleteRequest(
            "slug-article"
        )))
            .Should().ThrowAsync<NotFoundException>();
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

        await this.Invoking(x => x.Act(new ArticleDeleteRequest(
            "test-title"
        )))
            .Should().ThrowAsync<ForbiddenException>();
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

        await Act(new ArticleDeleteRequest("test-title"));

        (await _context.Articles.AnyAsync()).Should().BeFalse();
        (await _context.Comments.AnyAsync()).Should().BeFalse();
    }
}