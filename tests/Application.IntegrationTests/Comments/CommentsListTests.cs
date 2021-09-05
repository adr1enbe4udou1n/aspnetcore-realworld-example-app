using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Comments
{
    public class CommentsListTests : TestBase
    {
        public CommentsListTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task CanListAllCommentsOfNotExistingArticle()
        {
            await Act(() =>
                _mediator.Invoking(m => m.Send(new CommentsListQuery("test-title")))
                    .Should().ThrowAsync<NotFoundException>()
            );
        }

        [Fact]
        public async Task CanListAllCommentsOfArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            });

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var comments = new List<string>();

            for (int i = 1; i <= 5; i++)
            {
                comments.Add($"Test Comment {i}");
            }

            foreach (var c in comments)
            {
                await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
                {
                    Body = $"This is John, {c} !",
                }));
            }

            await ActingAs(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Bio = "My Bio",
                Image = "https://i.pravatar.cc/300"
            });

            foreach (var c in comments)
            {
                await _mediator.Send(new CommentCreateCommand("test-title", new CommentCreateDTO
                {
                    Body = $"This is Jane, {c} !",
                }));
            }

            var response = await Act(() =>
                _mediator.Send(new CommentsListQuery("test-title"))
            );

            response.Comments.Count().Should().Be(10);

            response.Comments.First().Should().BeEquivalentTo(new CommentDTO
            {
                Body = "This is Jane, Test Comment 5 !",
                Author = new AuthorDTO
                {
                    Username = "Jane Doe",
                    Bio = "My Bio",
                    Image = "https://i.pravatar.cc/300"
                },
            }, options => options.Excluding(x => x.Id).Excluding(x => x.CreatedAt).Excluding(x => x.UpdatedAt));
        }
    }
}
