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
            await this.Invoking(x => x.Act(new CommentDeleteRequest("slug-article", 1)))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CannotDeleteNotExistingComment()
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

            await this.Invoking(x => x.Act(new CommentDeleteRequest(
                "test-title", 1
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CannotDeleteCommentWithInexistingArticle()
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

            var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you !",
            }));

            await this.Invoking(x => x.Act(new CommentDeleteRequest(
                "slug-article", response.Comment.Id
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CannotDeleteCommentWithBadArticle()
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

            await Mediator.Send(new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = "Other Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you !",
            }));

            await this.Invoking(x => x.Act(new CommentDeleteRequest(
                "other-title", response.Comment.Id
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CannotDeleteCommentOfOtherAuthor()
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

            var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you !",
            }));

            await ActingAs(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            });

            await this.Invoking(x => x.Act(new CommentDeleteRequest(
                "test-title", response.Comment.Id
            )))
                .Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CanDeleteOwnComment()
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

            var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you !",
            }));

            await Act(new CommentDeleteRequest("test-title", response.Comment.Id));

            (await Context.Comments.AnyAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task CanDeleteAllCommentsOfOwnArticle()
        {
            var user = await ActingAs(new User
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

            await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you !",
            }));

            await ActingAs(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            });

            var response = await Mediator.Send(new NewCommentRequest("test-title", new NewCommentDTO
            {
                Body = "Thank you John !",
            }));

            await CurrentUser.SetIdentifier(user.Id);

            await Act(new CommentDeleteRequest("test-title", response.Comment.Id));

            (await Context.Comments.CountAsync()).Should().Be(1);
        }
    }
}
