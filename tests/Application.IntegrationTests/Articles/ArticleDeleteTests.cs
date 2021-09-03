using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticleDeleteTests : TestBase
    {
        public ArticleDeleteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotDeleteArticle()
        {
            await _mediator.Invoking(m => m.Send(new ArticleDeleteCommand("slug")))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CanDeleteArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleDeleteCommand(
                "slug-article"
            ));

            // TODO
        }
    }
}