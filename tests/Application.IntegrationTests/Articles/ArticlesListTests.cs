using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Articles
{
    public class ArticlesListTests : TestBase
    {
        public ArticlesListTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanPaginateArticles()
        {
            await CreateArticles();

            var response = await _mediator.Send(new ArticlesListQuery
            {
                Limit = 30,
                Offset = 10
            });

            response.Articles.Count().Should().Be(20);
            response.ArticlesCount.Should().Be(50);

            response.Articles.First().Should().BeEquivalentTo(new ArticleDTO
            {
                Title = "Jane Doe - Test Title 10",
                Description = "Test Description",
                Body = "Test Body",
                Author = new AuthorDTO
                {
                    Username = "Jane Doe"
                },
                TagList = new List<string> { "Test Tag 1", "Test Tag 2" },
                Favorited = false,
                FavoritesCount = 0,
            }, options => options
                .Excluding(x => x.Slug)
                .Excluding(x => x.CreatedAt)
                .Excluding(x => x.UpdatedAt)
            );
        }

        [Fact]
        public async Task CanFilterByAuthorArticles()
        {
            await CreateArticles();

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
            await CreateArticles();

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
            await CreateArticles();

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
            await CreateArticles();

            var response = await _mediator.Send(new ArticlesFeedQuery
            {
                Limit = 10,
                Offset = 0
            });

            // TODO
        }

        private async Task CreateArticles()
        {
            await CreateArticlesForAuthor(new User
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
            }, 30);

            await CreateArticlesForAuthor(new User
            {
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
            }, 20);
        }

        private async Task CreateArticlesForAuthor(User author, int count)
        {
            await ActingAs(author);

            var articles = new List<string>();

            for (int i = 1; i <= count; i++)
            {
                articles.Add($"{author.Name} - Test Title {i}");
            }

            foreach (var a in articles)
            {
                await _mediator.Send(new ArticleCreateCommand(
                    new ArticleCreateDTO
                    {
                        Title = a,
                        Description = "Test Description",
                        Body = "Test Body",
                        TagList = new List<string> { "Test Tag 1", "Test Tag 2" }
                    }
                ));
            }
        }
    }
}