using Xunit;

namespace Conduit.IntegrationTests;

[CollectionDefinition("Test collection")]
public class SharedTestCollectionFixture : ICollectionFixture<ConduitApiFactory>
{
}