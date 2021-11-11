using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Profiles.Queries;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles;

public class ArticleUpdateTests : TestBase
{
    public ArticleUpdateTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

    public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new UpdateArticleDTO {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            } },
        };

    [Theory]
    [MemberData(nameof(Data))]
    public async Task Cannot_Update_Article_With_Invalid_Data(UpdateArticleDTO article)
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await this.Invoking(x => x.Act(new UpdateArticleRequest(
            "test-title", article
        )))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Cannot_Update_Non_Existent_Article()
    {
        await ActingAs(new User
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
        });

        await this.Invoking(x => x.Act(new UpdateArticleRequest(
            "slug-article",
            new UpdateArticleDTO
            {
                Title = "New Title",
                Description = "New Description",
                Body = "New Body",
            }
        )))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Guest_Cannot_Update_Article()
    {
        await this.Invoking(x => x.Act(new UpdateArticleRequest(
            "slug-article", new UpdateArticleDTO()
        )))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Cannot_Update_Article_Of_Other_Author()
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

        await ActingAs(new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
        });

        await this.Invoking(x => x.Act(
            new UpdateArticleRequest(
                "test-title",
                new UpdateArticleDTO
                {
                    Title = "New Title",
                    Description = "New Description",
                    Body = "New Body",
                }
            )))
                .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Can_Update_Own_Article()
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

        var response = await Act(new UpdateArticleRequest(
            "test-title",
            new UpdateArticleDTO
            {
                Title = "New Title",
                Description = "New Description",
            }
        ));

        response.Article.Should().BeEquivalentTo(new ArticleDTO
        {
            Title = "New Title",
            Description = "New Description",
            Body = "Test Body",
            Slug = "test-title",
            Author = new ProfileDTO
            {
                Username = "John Doe",
            },
            TagList = new List<string>(),
        }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

        (await Context.Articles.AnyAsync(x => x.Title == "New Title")).Should().BeTrue();
    }
}