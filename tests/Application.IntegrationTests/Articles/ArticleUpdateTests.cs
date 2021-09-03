using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleUpdateTests : TestBase
    {
        public ArticleUpdateTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotUpdateArticle()
        {
            await _mediator.Invoking(m => m.Send(new ArticleUpdateCommand(
                "slug-article", new ArticleUpdateDTO()
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

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