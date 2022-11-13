using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Comments;

public class CommentDeleteTests : TestBase
{
    public CommentDeleteTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Guest_Cannot_Delete_Comment()
    {
        var response = await Act(HttpMethod.Delete, "/articles/test-title/comments/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cannot_Delete_Non_Existent_Comment()
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

        var response = await Act(HttpMethod.Delete, "/articles/test-title/comments/1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_With_Non_Existent_Article()
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

        var r = await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        var response = await Act(HttpMethod.Delete, $"/articles/slug-article/comments/{r.Comment.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_With_Bad_Article()
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

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Other Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var r = await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        var response = await Act(HttpMethod.Delete, $"/articles/slug-article/comments/{r.Comment.Id}", new CommentDeleteRequest(
            "other-title", r.Comment.Id
        ));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_Of_Other_Author()
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

        var r = await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await Act(HttpMethod.Delete, $"/articles/test-title/comments/{r.Comment.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Can_Delete_Own_Comment()
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

        var response = await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await Act(HttpMethod.Delete, $"/articles/test-title/comments/{response.Comment.Id}");

        (await _context.Comments.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Can_Delete_All_Comments_Of_Own_Article()
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

        await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you John !",
        }));

        await Act(HttpMethod.Delete, $"/articles/test-title/comments/{response.Comment.Id}");

        (await _context.Comments.CountAsync()).Should().Be(1);
    }
}