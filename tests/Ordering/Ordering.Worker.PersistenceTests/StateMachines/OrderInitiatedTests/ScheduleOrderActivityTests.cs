using FluentAssertions;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.PersistenceTests.TestBase;
using Ordering.Worker.StateMachines.Activities.Initialize;
using Shared.IntegrationEvents.Ordering;
using Ordering.Worker.StateMachines.Events;
using Xunit;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

public class ScheduleOrderActivityTests : WorkerPersistenceTestBase
{
    [Fact]
    public async Task Should_schedule_reminder_and_cancel_and_publish_order_paid()
    {
        await ResetDatabaseAsync();

        // Arrange
        var orderId = Guid.NewGuid();

        await PublishEventAsync(new OrderInitiated
        {
            Id = orderId,
            Date = DateTime.UtcNow
        });

        // منتظر پردازش Saga و Activity
        await Task.Delay(500);

        // Assert: Saga ایجاد شده باشد
        var saga = await DbContext.Set<OrderState>().FindAsync(orderId);
        saga.Should().NotBeNull();
        saga!.ReminderScheduleTokenId.Should().NotBeNull();
        saga.CancelScheduleTokenId.Should().NotBeNull();

        // Assert: پیام OrderPaid منتشر شده باشد
        var orderPaidPublished = await SagaHarness.Consumed.Any<OrderPaid>();
        orderPaidPublished.Should().BeTrue();
    }
}