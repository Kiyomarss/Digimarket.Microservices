namespace Ordering.Worker.PersistenceTests.Fixtures
{
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<OrderStateMachineFixture> { }
}