using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Comments;

public class CommentDeleteTests : TestBase
{
    [Test]
    public async Task Guest_Cannot_Delete_Comment()
    {
        await this.Invoking(x => x.Act(new CommentDeleteRequest("slug-article", 1)))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Cannot_Delete_Non_Existent_Comment()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await this.Invoking(x => x.Act(new CommentDeleteRequest(
            "test-title", 1
        )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Cannot_Delete_Comment_With_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await this.Invoking(x => x.Act(new CommentDeleteRequest(
            "slug-article", response.Comment.Id
        )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Cannot_Delete_Comment_With_Bad_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Other Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await this.Invoking(x => x.Act(new CommentDeleteRequest(
            "other-title", response.Comment.Id
        )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Cannot_Delete_Comment_Of_Other_Author()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        await this.Invoking(x => x.Act(new CommentDeleteRequest(
            "test-title", response.Comment.Id
        )))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Test]
    public async Task Can_Delete_Own_Comment()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await Act(new CommentDeleteRequest("test-title", response.Comment.Id));

        (await Context.Comments.AnyAsync()).Should().BeFalse();
    }

    [Test]
    public async Task Can_Delete_All_Comments_Of_Own_Article()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await Mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you John !",
        }));

        await CurrentUser.SetIdentifier(user.Id);

        await Act(new CommentDeleteRequest("test-title", response.Comment.Id));

        (await Context.Comments.CountAsync()).Should().Be(1);
    }
}