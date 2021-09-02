using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Comments.Queries;
using Xunit;

namespace Application.IntegrationTests.Comments
{
    public class CommentsListTests : TestBase
    {
        public CommentsListTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanListAllCommentsOfArticle()
        {
            // TODO

            var response = await _mediator.Send(new CommentsListQuery("slug-article"));

            // TODO
        }
    }
}