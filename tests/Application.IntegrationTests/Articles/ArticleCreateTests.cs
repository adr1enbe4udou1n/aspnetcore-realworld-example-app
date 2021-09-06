using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles
{
    public class ArticleCreateTests : TestBase
    {
        public ArticleCreateTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new ArticleCreateDTO {
                Title = "",
                Description = "Test Description",
                Body = "Test Body",
            } },
            new [] { new ArticleCreateDTO {
                Title = "Test Title",
                Description = "",
                Body = "Test Body",
            } },
            new [] { new ArticleCreateDTO {
                Title = "Test Title",
                Description = "Test Description",
                Body = "",
            } },
            new [] { new ArticleCreateDTO {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task CannotCreateArticleWithInvalidData(ArticleCreateDTO article)
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Existing Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            await this.Invoking(x => x.Act(new ArticleCreateCommand(article)))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task GuestCannotCreateArticle()
        {
            await this.Invoking(x => x.Act(new ArticleCreateCommand(
                new ArticleCreateDTO()
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CanCreateArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            });

            await Context.Tags.AddAsync(new Tag
            {
                Name = "Existing Tag"
            });
            await Context.SaveChangesAsync();

            var response = await Act(
                new ArticleCreateCommand(
                    new ArticleCreateDTO
                    {
                        Title = "Test Article",
                        Description = "Test Description",
                        Body = "Test Body",
                        TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" }
                    }
                )
            );

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Title = "Test Article",
                Description = "Test Description",
                Body = "Test Body",
                Slug = "test-article",
                Author = new AuthorDTO
                {
                    Username = "John Doe",
                    Bio = "My Bio",
                    Image = "https://i.pravatar.cc/300"
                },
                TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" },
            }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

            (await Context.Articles.AnyAsync()).Should().BeTrue();
            (await Context.Tags.CountAsync()).Should().Be(3);
        }
    }
}
