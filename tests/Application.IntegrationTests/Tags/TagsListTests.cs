using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.Tags.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Tags
{
    public class TagListTests : TestBase
    {
        public TagListTests(Startup factory, ITestOutputHelper output) : base(factory, output) { }

        [Fact]
        public async Task CanListAllTags()
        {
            await _context.Tags.AddRangeAsync(
                new Tag { Name = "Tag3" },
                new Tag { Name = "Tag2" },
                new Tag { Name = "Tag1" }
            );
            await _context.SaveChangesAsync();

            var response = await Act(() =>
                _mediator.Send(new TagsListQuery())
            );

            response.Tags.Should().Equal("Tag1", "Tag2", "Tag3");
        }
    }
}
