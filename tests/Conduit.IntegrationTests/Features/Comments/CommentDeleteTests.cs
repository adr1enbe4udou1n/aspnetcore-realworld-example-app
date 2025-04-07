using System.Net;

using Conduit.Application.Features.Articles.Commands;
using Conduit.Application.Features.Comments.Commands;
using Conduit.Domain.Entities;


using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Comments;

public class CommentDeleteTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Guest_Cannot_Delete_Comment()
    {
        var response = await Act(HttpMethod.Delete, "/articles/test-title/comments/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Non_Existent_Comment()
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

        var response = await Act(HttpMethod.Delete, "/articles/test-title/comments/1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_With_Non_Existent_Article()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
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

        var comment = new Comment
        {
            Body = "Thank you !",
            Article = article,
            Author = user,
        };

        Context.Comments.Add(comment);

        await Context.SaveChangesAsync();

        var response = await Act(HttpMethod.Delete, $"/articles/slug-article/comments/{comment.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_With_Bad_Article()
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

        var article = new Article
        {
            Title = "Other Title",
            Description = "Test Description",
            Body = "Test Body",
            Slug = "other-title",
            Author = user,
        };

        Context.Articles.Add(article);

        var comment = new Comment
        {
            Body = "Thank you !",
            Article = article,
            Author = user,
        };

        Context.Comments.Add(comment);

        await Context.SaveChangesAsync();

        var response = await Act(HttpMethod.Delete, $"/articles/slug-article/comments/{comment.Id}", new CommentDeleteCommand(
            "other-title", comment.Id
        ));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_Delete_Comment_Of_Other_Author()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
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

        var comment = new Comment
        {
            Body = "Thank you !",
            Article = article,
            Author = user,
        };

        Context.Comments.Add(comment);

        await Context.SaveChangesAsync();

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var response = await Act(HttpMethod.Delete, $"/articles/test-title/comments/{comment.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Can_Delete_Own_Comment()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
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

        var comment = new Comment
        {
            Body = "Thank you !",
            Article = article,
            Author = user,
        };

        Context.Comments.Add(comment);

        await Context.SaveChangesAsync();

        await Act(HttpMethod.Delete, $"/articles/test-title/comments/{comment.Id}");

        Assert.False(await Context.Comments.AnyAsync());
    }

    [Fact]
    public async Task Can_Delete_All_Comments_Of_Own_Article()
    {
        var user = await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
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

        Context.Comments.Add(new Comment
        {
            Body = "Thank you !",
            Article = article,
            Author = user,
        });

        await Context.SaveChangesAsync();

        user = await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        var comment = new Comment
        {
            Body = "Thank you John !",
            Article = article,
            Author = user,
        };

        Context.Comments.Add(comment);

        await Act(HttpMethod.Delete, $"/articles/test-title/comments/{comment.Id}");

        Assert.Equal(1, await Context.Comments.CountAsync());
    }
}