using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Tags.Queries;
using Domain.Entities;
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
                new Tag { Name = "Tag1" },
                new Tag { Name = "Tag2" },
                new Tag { Name = "Tag3" },
            });
            await _context.SaveChangesAsync();

            var response = await _mediator.Send(new TagsListQuery());

            Assert.Contains("Tag1", response.Tags);
            Assert.Contains("Tag2", response.Tags);
            Assert.Contains("Tag3", response.Tags);
        }
    }
}