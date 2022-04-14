using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Comments;

public class CommentCreateTests : TestBase
{

    private static IEnumerable<TestCaseData> InvalidComments()
    {
        yield return new TestCaseData(new NewCommentDTO
        {
            Body = "",
        });
    }

    [Test, TestCaseSource(nameof(InvalidComments))]
    public async Task Cannot_Create_Comment_With_Invalid_Data(NewCommentDTO comment)
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

        await this.Invoking(x => x.Act(new NewCommentRequest("test-title", comment)))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Cannot_Create_Comment_To_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await this.Invoking(x => x.Act(new NewCommentRequest(
            "slug-article", new NewCommentDTO
            {
                Body = "Test Body",
            }
        )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Guest_Cannot_Create_Comment()
    {
        await this.Invoking(x => x.Act(new NewCommentRequest(
            "slug-article", new NewCommentDTO()
        )))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Can_Create_Comment()
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
            }
        ));

        var response = await Act(new NewCommentRequest("test-title", new NewCommentDTO
        {
            Body = "Thank you !",
        }));

        response.Comment.Should().BeEquivalentTo(new CommentDTO
        {
            Body = "Thank you !",
            Author = new ProfileDTO
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await _context.Comments.AnyAsync()).Should().BeTrue();
    }
}