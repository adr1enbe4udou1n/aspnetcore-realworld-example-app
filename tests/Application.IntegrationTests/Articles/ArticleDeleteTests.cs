using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles
{
    public class ArticleDeleteTests : TestBase
    {
        public ArticleDeleteTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task GuestCannotDeleteArticle()
        {
            await Act(() =>
                _mediator.Invoking(m => m.Send(new ArticleDeleteCommand("slug-article")))
                    .Should().ThrowAsync<UnauthorizedException>()
            );
        }

        [Fact]
        public async Task CannotDeleteNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Act(() =>
                _mediator.Invoking(m => m.Send(new ArticleDeleteCommand(
                    "slug-article"
                )))
                    .Should().ThrowAsync<NotFoundException>()
            );
        }

        [Fact]
        public async Task CannotDeleteArticleOfOtherAuthor()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Send(new ArticleCreateCommand(
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

            await Act(() =>
                _mediator.Invoking(m => m.Send(new ArticleDeleteCommand(
                    "test-title"
                )))
                    .Should().ThrowAsync<ForbiddenException>()
            );
        }

        [Fact]
        public async Task CanDeleteOwnArticleWithAllComments()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            for (int i = 1; i <= 5; i++)
            {
                await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
                {
                    Body = $"This is John, Test Comment {i} !",
                }));
            }

            await _mediator.Send(new ArticleFavoriteCommand("test-title", true));

            await Act(() =>
                _mediator.Send(new ArticleDeleteCommand(
                    "test-title"
                ))
            );

            (await _context.Articles.AnyAsync()).Should().BeFalse();
            (await _context.Comments.AnyAsync()).Should().BeFalse();
        }
    }
}
