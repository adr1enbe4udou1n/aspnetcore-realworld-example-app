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

namespace Application.IntegrationTests.Comments
{
    public class CommentDeleteTests : TestBase
    {
        public CommentDeleteTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task GuestCannotDeleteComment()
        {
            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentDeleteCommand("slug-article", 1)))
                    .Should().ThrowAsync<UnauthorizedException>()
            );
        }

        [Fact]
        public async Task CannotDeleteNotExistingComment()
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

            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                    "test-title", 1
                )))
                    .Should().ThrowAsync<NotFoundException>()
            );
        }

        [Fact]
        public async Task CannotDeleteCommentWithInexistingArticle()
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

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                    "slug-article", response.Comment.Id
                )))
                    .Should().ThrowAsync<NotFoundException>()
            );
        }

        [Fact]
        public async Task CannotDeleteCommentWithBadArticle()
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

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Other Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                    "other-title", response.Comment.Id
                )))
                    .Should().ThrowAsync<NotFoundException>()
            );
        }

        [Fact]
        public async Task CannotDeleteCommentOfOtherAuthor()
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

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await ActingAs(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            });

            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                    "test-title", response.Comment.Id
                )))
                    .Should().ThrowAsync<ForbiddenException>()
            );
        }

        [Fact]
        public async Task CanDeleteOwnComment()
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

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await Act(() =>
                _mediator.Send(new CommentDeleteCommand("test-title", response.Comment.Id))
            );

            (await _context.Comments.AnyAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task CanDeleteAllCommentsOfOwnArticle()
        {
            var user = await ActingAs(new User
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

            await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await ActingAs(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            });

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you John !",
            }));

            await _currentUser.SetIdentifier(user.Id);

            await Act(() =>
                _mediator.Send(new CommentDeleteCommand("test-title", response.Comment.Id))
            );

            (await _context.Comments.CountAsync()).Should().Be(1);
        }
    }
}