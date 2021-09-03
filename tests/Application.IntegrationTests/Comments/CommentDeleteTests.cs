using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Comments.Commands;
using FluentAssertions;
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
        public async Task CanDeleteComment()
        {
            // TODO

            var response = await _mediator.Send(new CommentDeleteCommand("slug-article", 1));

            // TODO
        }
    }
}