using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Comments.Commands;
using Conduit.Application.Features.Comments.Queries;
using Conduit.Domain.Entities;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Comments;

public class CommentsListTests : TestBase
{
    public CommentsListTests(ConduitApiFactory factory, ITestOutputHelper output) : base(factory, output) { }

    [Fact]
    public async Task Cannot_List_All_Comments_Of_Non_Existent_Article()
    {
        var response = await Act(HttpMethod.Get, "/articles/test-title/comments");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Can_List_All_Comments_Of_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        await Mediator.Send(new NewArticleCommand(
            new NewArticleDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var comments = new List<string>();

        for (var i = 1; i <= 5; i++)
        {
            comments.Add($"Test Comment {i}");
        }

        foreach (var c in comments)
        {
            await Mediator.Send(new NewCommentCommand("test-title", new NewCommentDto
            {
                Body = $"This is John, {c} !",
            }));
        }

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        foreach (var c in comments)
        {
            await Mediator.Send(new NewCommentCommand("test-title", new NewCommentDto
            {
                Body = $"This is Jane, {c} !",
            }));
        }

        var response = await Act<MultipleCommentsResponse>(HttpMethod.Get, "/articles/test-title/comments");

        response.Comments.Count().Should().Be(10);

        response.Comments.First().Should().BeEquivalentTo(new
        {
            Body = "This is Jane, Test Comment 5 !",
            Author = new
            {
                Username = "Jane Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        });
    }
}