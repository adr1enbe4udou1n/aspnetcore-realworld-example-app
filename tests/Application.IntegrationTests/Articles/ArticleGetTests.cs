using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles
{
    public class ArticleGetTests : TestBase
    {
        public ArticleGetTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task CannotGetNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(new ArticleGetQuery(
                "slug-article"
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanGetArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            });

            await Mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2" }
                }
            ));

            var response = await Act(new ArticleGetQuery("test-title"));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
                Slug = "test-title",
                Author = new AuthorDTO
                {
                    Username = "John Doe",
                    Bio = "My Bio",
                    Image = "https://i.pravatar.cc/300"
                },
                TagList = new List<string> { "Test Tag 1", "Test Tag 2" },
                Favorited = false,
                FavoritesCount = 0,
            }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));
        }
    }
}
