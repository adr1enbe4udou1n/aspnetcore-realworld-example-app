using System.Net;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Features.Comments;

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

        await _mediator.Send(new NewArticleRequest(
            new NewArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
            }
        ));

        var comments = new List<string>();

        for (int i = 1; i <= 5; i++)
        {
            comments.Add($"Test Comment {i}");
        }

        foreach (var c in comments)
        {
            await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
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
            await _mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = $"This is Jane, {c} !",
            }));
        }

        var response = await Act<MultipleCommentsResponse>(HttpMethod.Get, "/articles/test-title/comments");

        response.Comments.Count().Should().Be(10);

        response.Comments.First().Should().BeEquivalentTo(new CommentDTO
        {
            Body = "This is Jane, Test Comment 5 !",
            Author = new ProfileDTO
            {
                Username = "Jane Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));
    }
}