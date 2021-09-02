using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Articles.Queries;
using Application.Features.Tags.Queries;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticlesListTests : TestBase
    {
        public ArticlesListTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanPaginateArticles()
        {
            // TODO

            var response = await _mediator.Send(new ArticlesListQuery
            {
                Limit = 10,
                Offset = 0
            });

            // TODO
        }

        [Fact]
        public async Task CanFilterByAuthorArticles()
        {
            // TODO

            var response = await _mediator.Send(new ArticlesListQuery
            {
                Limit = 10,
                Offset = 0,
                Author = "Author1"
            });

            // TODO
        }

        [Fact]
        public async Task CanFilterByTagArticles()
        {
            // TODO

            var response = await _mediator.Send(new ArticlesListQuery
            {
                Limit = 10,
                Offset = 0,
                Tag = "Tag1",
            });

            // TODO
        }

        [Fact]
        public async Task CanFilterByFavoritedArticles()
        {
            // TODO

            var response = await _mediator.Send(new ArticlesListQuery
            {
                Limit = 10,
                Offset = 0,
                Favorited = "User1",
            });

            // TODO
        }

        [Fact]
        public async Task CanListFollowedAuthorArticles()
        {
            // TODO

            var response = await _mediator.Send(new ArticlesFeedQuery
            {
                Limit = 10,
                Offset = 0
            });

            // TODO
        }
    }
}