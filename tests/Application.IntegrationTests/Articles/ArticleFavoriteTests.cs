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

namespace Application.IntegrationTests.Articles
{
    public class ArticleFavoriteTests : TestBase
    {
        public ArticleFavoriteTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task GuestCannotFavoriteArticle()
        {
            await _mediator.Invoking(m => m.Send(new ArticleFavoriteCommand(
                "slug-article", true
            )))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CannotFavoriteNotExistingArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Invoking(m => m.Send(new ArticleFavoriteCommand(
                "slug-article", true
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanFavoriteArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            var response = await _mediator.Send(new ArticleFavoriteCommand("test-title", true));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Favorited = true,
                FavoritesCount = 1,
            }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

            (await _context.Set<ArticleFavorite>().CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CanUnfavoriteArticle()
        {
            await ActingAs(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            });

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            await _mediator.Send(new ArticleFavoriteCommand("test-title", true));

            var response = await _mediator.Send(new ArticleFavoriteCommand("test-title", false));

            response.Article.Should().BeEquivalentTo(new ArticleDTO
            {
                Favorited = false,
                FavoritesCount = 0,
            }, options => options.Including(x => x.Favorited).Including(x => x.FavoritesCount));

            (await _context.Set<ArticleFavorite>().CountAsync()).Should().Be(0);
        }
    }
}