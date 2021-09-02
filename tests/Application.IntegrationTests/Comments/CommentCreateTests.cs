using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentCreateTests : TestBase
    {
        public CommentCreateTests(Startup factory) : base(factory) { }

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