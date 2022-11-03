using Xunit;

namespace Application.IntegrationTests;

[CollectionDefinition("Test collection")]
public class SharedTestCollection : ICollectionFixture<ConduitApiFactory>
{
}