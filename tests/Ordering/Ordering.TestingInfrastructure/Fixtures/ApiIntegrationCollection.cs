using Xunit;

namespace Ordering.TestingInfrastructure.Fixtures;

[CollectionDefinition("ApiIntegration")]
public class ApiIntegrationCollection : ICollectionFixture<OrderingAppFactory>
{
}
// Defines the "ApiIntegration" test collection and shares a single
// OrderingAppFactory instance across all tests in this collection.
// Used by xUnit via reflection (no direct references in code).