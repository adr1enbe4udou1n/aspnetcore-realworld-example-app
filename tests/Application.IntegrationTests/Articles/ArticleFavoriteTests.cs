using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Articles
{
    public class ArticleFavoriteTests : TestBase
    {
        public ArticleFavoriteTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task Guest_Cannot_Favorite_Article()
        {
            await this.Invoking(x => x.Act(new ArticleFavoriteRequest(
                "slug-article", true
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task Cannot_Favorite_Non_Existent_Article()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await this.Invoking(x => x.Act(new ArticleFavoriteRequest(
                "slug-article", true
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Can_Favorite_Article()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Mediator.Send(new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var response = await Act(new ArticleFavoriteRequest("test-title", true));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Favorited = true,
                FavoritesCount = 1,
            }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

            (await Context.Set<ArticleFavorite>().CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Can_Unfavorite_Article()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await Mediator.Send(new NewArticleRequest(
                new NewArticleDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            await Mediator.Send(new ArticleFavoriteRequest("test-title", true));

            var response = await Act(new ArticleFavoriteRequest("test-title", false));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Favorited = false,
                FavoritesCount = 0,
            }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

            (await Context.Set<ArticleFavorite>().CountAsync()).Should().Be(0);
        }
    }
}
