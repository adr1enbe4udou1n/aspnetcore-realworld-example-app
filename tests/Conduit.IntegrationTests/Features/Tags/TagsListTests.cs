using Conduit.Application.Features.Tags.Queries;
using Conduit.Domain.Entities;


using Xunit;
using Xunit.Abstractions;

namespace Conduit.IntegrationTests.Features.Tags;

public class TagsListTests(ConduitApiFixture factory, ITestOutputHelper output) : TestBase(factory, output)
{

    [Fact]
    public async Task Can_List_All_Tags()
    {
        await Context.Tags.AddRangeAsync(
            new Tag { Name = "Tag3" },
            new Tag { Name = "Tag2" },
            new Tag { Name = "Tag1" }
        );
        await Context.SaveChangesAsync();

        var response = await Act<TagsResponse>(HttpMethod.Get, "/tags");

        Assert.Equal(["Tag1", "Tag2", "Tag3"], response.Tags);
    }
}