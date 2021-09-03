using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleDeleteTests : TestBase
    {
        public ArticleDeleteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotDeleteArticle()
        {
            await _mediator.Invoking(m => m.Send(new ArticleDeleteCommand("slug-article")))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CannotDeleteNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Invoking(m => m.Send(new ArticleDeleteCommand(
                "slug-article"
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanDeleteArticle()
        {
            var user = await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _context.Articles.AddAsync(new Article
            {
                Title = "Test Title",
                Description = "Test Description",
                Body = "Test Body",
                Slug = _slugifier.Generate("Test Title"),
                Author = user,
            });
            await _context.SaveChangesAsync();

            await _mediator.Send(new ArticleDeleteCommand(
                "test-title"
            ));

            (await _context.Articles.AnyAsync()).Should().BeFalse();
        }
    }
}