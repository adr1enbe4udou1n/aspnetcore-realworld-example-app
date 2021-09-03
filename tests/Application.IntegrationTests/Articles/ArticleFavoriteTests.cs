using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Articles.Commands;
using Domain.Entities;
using FluentAssertions;
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

            await _mediator.Send(new ArticleCreateCommand(
                new ArticleCreateDTO
                {
                    Title = "Test Title",
                    Description = "Test Description",
                    Body = "Test Body",
                }
            ));

            await _mediator.Invoking(m => m.Send(new ArticleFavoriteCommand(
                "slug-article", true
            )))
                .Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CanFavoriteArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleFavoriteCommand("slug-article", true));

            // TODO
        }

        [Fact]
        public async Task CanUnfavoriteArticle()
        {
            // TODO

            var response = await _mediator.Send(new ArticleFavoriteCommand("slug-article", false));

            // TODO
        }
    }
}