using System;
using System.Collections.Generic;
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
    public class ArticleUpdateTests : TestBase
    {
        public ArticleUpdateTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new ArticleUpdateDTO {
                Body = "",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task CannotUpdateArticleWithInvalidData(ArticleUpdateDTO article)
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(new ArticleUpdateCommand(
                "test-title", article
            )))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CannotUpdateNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(new ArticleUpdateCommand(
                "slug-article",
                new ArticleUpdateDTO
                {
                    Body = "New Body",
                }
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GuestCannotUpdateArticle()
        {
            await this.Invoking(x => x.Act(new ArticleUpdateCommand(
                "slug-article", new ArticleUpdateDTO()
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CannotUpdateArticleOfOtherAuthor()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
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
                new ArticleUpdateCommand(
                    "test-title",
                    new ArticleUpdateDTO
                    {
                        Body = "New Body",
                    }
                )))
                    .Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CanUpdateOwnArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var response = await Act(new ArticleUpdateCommand(
                "test-title",
                new ArticleUpdateDTO
                {
                    Body = "New Body",
                }
            ));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "New Body",
                Slug = "test-title",
                Author = new AuthorDTO
                {
                    Username = "John Doe",
                },
                TagList = new List<string>(),
            }, options => options.Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

            (await Context.Articles.AnyAsync(x => x.Body == "New Body")).Should().BeTrue();
        }
    }
}
