using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Articles.Commands;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleDeleteTests : TestBase
    {
        public ArticleDeleteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanCreateArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleDeleteCommand(
                "slug-article"
            ));

            // TODO
        }
    }
}