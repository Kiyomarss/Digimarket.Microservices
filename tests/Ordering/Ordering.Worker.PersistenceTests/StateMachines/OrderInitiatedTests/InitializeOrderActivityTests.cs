using FluentAssertions;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.PersistenceTests.Fixtures;
using Ordering.Worker.PersistenceTests.TestBase;
using Ordering.Worker.StateMachines.Activities.Initialize;
using Ordering.Worker.StateMachines.Events;
using Shared.IntegrationEvents.Ordering;
using Xunit;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

public class InitializeOrderActivityTests : WorkerPersistenceTestBase
{
    public InitializeOrderActivityTests(OrderingWorkerPersistenceFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Should_create_saga_and_publish_reduce_inventory_and_remove_basket()
    {
        await ResetDatabaseAsync();

        // Arrange
        var orderId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act: منتشر کردن رویداد OrderInitiated
        await PublishEventAsync(new OrderInitiated
        {
            Id = orderId,
            Date = date
        });

        // منتظر پردازش پیام توسط MassTransit و Saga
        await Task.Delay(500); // یا استفاده از Retry تا Saga در DB ایجاد شود

        // Assert: Saga ایجاد شده باشد
        var saga = await DbContext.Set<OrderState>().FindAsync(orderId);
        saga.Should().NotBeNull();
        saga!.Date.Should().Be(date);

        // Assert: پیام‌ها منتشر شده باشند
        var reduceInventoryPublished = await SagaHarness.Consumed.Any<ReduceInventory>();
        var removeBasketPublished = await SagaHarness.Consumed.Any<RemoveBasket>();

        reduceInventoryPublished.Should().BeTrue();
        removeBasketPublished.Should().BeTrue();
    }
}