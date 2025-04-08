using System.Net;

using Conduit.Application.Features.Comments.Queries;
using Conduit.Domain.Entities;


using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Comments;

public class CommentsListTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Cannot_List_All_Comments_Of_Non_Existent_Article()
    {
        var response = await Act(HttpMethod.Get, "/articles/test-title/comments");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Can_List_All_Comments_Of_Article()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        var article = new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = user,
        };

        Context.Articles.Add(article);

        var comments = new List<string>();

        for (var i = 1; i <= 5; i++)
        {
            comments.Add($"Test Comment {i}");
        }

        foreach (var c in comments)
        {
            Context.Comments.Add(new Comment
            {
                Body = $"This is John, {c} !",
                Article = article,
                Author = user,
            });
        }

        user = await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Bio = "My Bio",
            Image = "https://i.pravatar.cc/300"
        });

        foreach (var c in comments)
        {
            Context.Comments.Add(new Comment
            {
                Body = $"This is Jane, {c} !",
                Article = article,
                Author = user,
            });
        }

        await Context.SaveChangesAsync();

        var response = await Act<MultipleCommentsResponse>(HttpMethod.Get, "/articles/test-title/comments");

        Assert.Equal(10, response.Comments.Count());

        Assert.Equivalent(new
        {
            Body = "This is Jane, Test Comment 5 !",
            Author = new
            {
                Username = "Jane Doe",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            },
        }, response.Comments.First());
    }
}