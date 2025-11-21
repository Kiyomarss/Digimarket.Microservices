using Xunit;

namespace Ordering.TestingInfrastructure.Fixtures;

[CollectionDefinition("ApiIntegration")]
public class ApiIntegrationCollection : ICollectionFixture<OrderingAppFactory>
{
}