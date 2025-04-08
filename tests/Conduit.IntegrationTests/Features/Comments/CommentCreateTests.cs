using System.Net;

using Conduit.Application.Features.Comments.Commands;
using Conduit.Domain.Entities;
using Conduit.Presentation.Endpoints;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Comments;

public class InvalidComments : TheoryData<NewCommentDto>
{
    public InvalidComments()
    {
        Add(new NewCommentDto
        {
            Body = "",
        });
    }
}

public class CommentCreateTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Theory, ClassData(typeof(InvalidComments))]
    public async Task Cannot_Create_Comment_With_Invalid_Data(NewCommentDto comment)
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        Context.Articles.Add(new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        });

        await Context.SaveChangesAsync();

        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(comment));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Create_Comment_To_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(
            new NewCommentDto
            {
                Body = "Test Body",
            }
        ));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Guest_Cannot_Create_Comment()
    {
        var response = await Act(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(
            new NewCommentDto
            {
                Body = "Test Body",
            }
        ));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Can_Create_Comment()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        Context.Articles.Add(new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        });

        await Context.SaveChangesAsync();

        var response = await Act<SingleCommentResponse>(HttpMethod.Post, "/articles/test-title/comments", new NewCommentRequest(new NewCommentDto
        {
            Body = "Thank you !",
        }));

        Assert.Equivalent(new
        {
            Body = "Thank you !",
            Author = new
            {
                Username = "John Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, response.Comment);

        Assert.True(await Context.Set<Comment>().AnyAsync());
    }
}