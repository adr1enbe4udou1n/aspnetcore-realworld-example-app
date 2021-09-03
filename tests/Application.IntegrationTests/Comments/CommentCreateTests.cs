using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentCreateTests : TestBase
    {
        public CommentCreateTests(Startup factory) : base(factory) { }

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
            // TODO

            var response = await _mediator.Send(new CommentCreateCommand("slug-article", new CommentCreateDTO
            {
                Body = "Thank you !",
            }));

            // TODO
        }
    }
}