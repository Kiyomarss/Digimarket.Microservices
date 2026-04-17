using BuildingBlocks.IntegrationEvents;
using FluentAssertions;
using Ordering.TestingInfrastructure.Builders;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.PersistenceTests.Fixtures;

namespace Ordering.Worker.PersistenceTests.StateMachines.OrderInitiatedTests;

// public class ScheduleOrderActivityTests : OrderingWorkerPersistenceFixture
// {
//     [Fact]
//     public async Task Should_schedule_reminder_and_cancel_and_publish_order_paid()
//     {
//         await ResetDatabaseAsync();
//
//         // Arrange
//         var orderId = Guid.NewGuid();
//
//         await PublishEventAsync(new OrderInitiatedBuilder().WithId(orderId).Build());
//
//         // منتظر پردازش Saga و Activity
//         await Task.Delay(500);
//
//         // Assert: Saga ایجاد شده باشد
//         var saga = await DbContext.Set<OrderState>().FindAsync(orderId);
//         saga.Should().NotBeNull();
//         saga!.ReminderScheduleTokenId.Should().NotBeNull();
//         saga.CancelScheduleTokenId.Should().NotBeNull();
//
//         // Assert: پیام OrderPaid منتشر شده باشد
//         var orderPaidPublished = await SagaHarness.Consumed.Any<OrderPaid>();
//         orderPaidPublished.Should().BeTrue();
//     }
// }