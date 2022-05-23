using Application.Features.Tags.Queries;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Application.IntegrationTests.Features.Tags;

public class TagsListTests : TestBase
{
    [Test]
    public async Task Can_List_All_Tags()
    {
        await _context.Tags.AddRangeAsync(
            new Tag { Name = "Tag3" },
            new Tag { Name = "Tag2" },
            new Tag { Name = "Tag1" }
        );
        await _context.SaveChangesAsync();

        var response = await Act<TagsResponse>(HttpMethod.Get, "/tags");

        response.Tags.Should().Equal("Tag1", "Tag2", "Tag3");
    }
}