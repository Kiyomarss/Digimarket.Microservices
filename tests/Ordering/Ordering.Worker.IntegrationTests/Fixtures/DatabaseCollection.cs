namespace Ordering.Worker.IntegrationTests.Fixtures
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<OrderStateMachineFixture> { }
}