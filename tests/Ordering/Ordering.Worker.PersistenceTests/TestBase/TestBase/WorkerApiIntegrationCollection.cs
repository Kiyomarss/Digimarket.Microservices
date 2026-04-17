using Ordering.Worker.PersistenceTests.Fixtures;

namespace Ordering.Worker.PersistenceTests.TestBase.TestBase;

[CollectionDefinition("WorkerIntegration")]
public class WorkerApiIntegrationCollection : ICollectionFixture<WorkerAppFactory>
{
}
// Defines the "ApiIntegration" test collection and shares a single
// WorkerAppFactory instance across all tests in this collection.
// Used by xUnit via reflection (no direct references in code).