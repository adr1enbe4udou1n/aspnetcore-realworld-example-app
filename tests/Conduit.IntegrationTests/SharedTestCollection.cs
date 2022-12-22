using Xunit;

namespace Application.IntegrationTests;

[CollectionDefinition("Test collection")]
public class SharedTestCollectionFixture : ICollectionFixture<ConduitApiFactory>
{
}