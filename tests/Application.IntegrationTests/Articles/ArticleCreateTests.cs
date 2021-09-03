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

namespace Application.IntegrationTests.Articles
{
    public class ArticleCreateTests : TestBase
    {
        public ArticleCreateTests(Startup factory) : base(factory) { }

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
            var user = await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _context.Articles.AddAsync(new Article
            {
                Title = "Existing Title",
                Description = "Test Description",
                Body = "Test Body",
                Slug = _slugifier.Generate("Existing Title"),
                Author = user,
            });
            await _context.SaveChangesAsync();

            await _mediator.Invoking(m => m.Send(new ArticleCreateCommand(article)))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task GuestCannotCreateArticle()
        {
            await _mediator.Invoking(m => m.Send(new ArticleCreateCommand(
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

            await _context.Tags.AddAsync(new Tag
            {
                Name = "Existing Tag"
            });
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2", "Existing Tag" }
                }
            ));

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

            (await _context.Articles.AnyAsync()).Should().BeTrue();
            (await _context.Tags.CountAsync()).Should().Be(3);
        }
    }
}