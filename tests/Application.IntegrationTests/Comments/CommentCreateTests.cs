using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Queries;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentCreateTests : TestBase
    {
        public CommentCreateTests(Startup factory) : base(factory) { }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new [] { new CommentCreateDTO {
                Body = "",
            } },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task CannotCreateCommentWithInvalidData(CommentCreateDTO comment)
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

            await _mediator.Invoking(m => m.Send(new CommentCreateCommand("test-title", comment)))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CannotCreateCommentToNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Invoking(m => m.Send(new CommentCreateCommand(
                "slug-article", new CommentCreateDTO
                {
                    Body = "Test Body",
                }
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GuestCannotCreateComment()
        {
            await _mediator.Invoking(m => m.Send(new CommentCreateCommand(
                "slug-article", new CommentCreateDTO()
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CanCreateComment()
        {
            var user = await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
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

            response.Comment.Should().BeEquivalentTo(new CommentDTO
            {
                Body = "Thank you !",
                Author = new AuthorDTO
                {
                    Username = "John Doe",
                    Bio = "My Bio",
                    Image = "https://i.pravatar.cc/300"
                },
            }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));

            (await _context.Comments.AnyAsync()).Should().BeTrue();
        }
    }
}