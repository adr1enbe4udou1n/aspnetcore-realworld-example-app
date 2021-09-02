using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Articles.Commands;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleUpdateTests : TestBase
    {
        public ArticleUpdateTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanUpdateArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleUpdateCommand(
                "slug-article",
                new ArticleUpdateDTO
                {
                    Body = "Test Body",
                }
            ));

            // TODO
        }
    }
}