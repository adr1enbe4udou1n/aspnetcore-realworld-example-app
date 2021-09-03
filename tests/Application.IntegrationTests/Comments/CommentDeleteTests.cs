using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Comments.Commands;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentDeleteTests : TestBase
    {
        public CommentDeleteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotDeleteComment()
        {
            await _mediator.Invoking(m => m.Send(new CommentDeleteCommand("slug-article", 1)))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CannotDeleteNotExistingComment()
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

            await _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                "test-title", 1
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CannotDeleteCommentWithInexistingArticle()
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

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                "slug-article", response.Comment.Id
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CannotDeleteCommentWithBadArticle()
        {
            var user = await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _context.Articles.AddRangeAsync(
                new Article
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                    Slug = _slugifier.Generate("Test Title"),
                    Author = user,
                },
                new Article
                {
                    Title = "Other Title",
                    Description = "Test Description",
                    Body = "Test Body",
                    Slug = _slugifier.Generate("Other Title"),
                    Author = user,
                }
            );
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await _mediator.Invoking(m => m.Send(new CommentDeleteCommand(
                "other-title", response.Comment.Id
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanDeleteComment()
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

            var response = await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            await _mediator.Send(new CommentDeleteCommand("test-title", response.Comment.Id));

            (await _context.Comments.AnyAsync()).Should().BeFalse();
        }
    }
}