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

namespace Application.IntegrationTests.Articles
{
    public class ArticleCreateTests : TestBase
    {
        public ArticleCreateTests(Startup factory) : base(factory) { }

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
            });

            var response = await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag 1", "Test Tag 2" }
                }
            ));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Title = "Test Article",
                Description = "Test Description",
                Body = "Test Body",
            });

            (await _context.Articles.AnyAsync()).Should().BeTrue();
        }
    }
}