using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Articles.Commands;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleCreateTests : TestBase
    {
        public ArticleCreateTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanCreateArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Article",
                    Description = "Test Description",
                    Body = "Test Body",
                    TagList = new List<string> { "Test Tag" }
                }
            ));

            // TODO
        }
    }
}