using Xunit;

namespace Ordering.Api.IntegrationTests.Fixtures;

[CollectionDefinition("ApiIntegration")]
public class ApiIntegrationCollection : ICollectionFixture<OrderingApiFactory>
{
}