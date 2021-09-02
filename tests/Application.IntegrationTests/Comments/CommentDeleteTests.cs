using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Comments.Commands;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentDeleteTests : TestBase
    {
        public CommentDeleteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanDeleteComment()
        {
            // TODO

            var response = await _mediator.Send(new CommentDeleteCommand("slug-article", 1));

            // TODO
        }
    }
}