using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Tags.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.IntegrationTests.Tags
{
    public class TagListTests : TestBase
    {
        public TagListTests(Startup factory) : base(factory) { }

        [Fact]
        public async Task CanListAllTags()
        {
            await _context.Tags.AddRangeAsync(new List<Tag>
            {
                new Tag { Name = "Tag3" },
                new Tag { Name = "Tag2" },
                new Tag { Name = "Tag1" },
            });
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new TagsListQuery());

            response.Tags.Should().Equal("Tag1", "Tag2", "Tag3");
        }
    }
}