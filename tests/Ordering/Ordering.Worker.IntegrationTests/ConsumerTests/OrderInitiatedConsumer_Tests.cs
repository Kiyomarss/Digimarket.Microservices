using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ordering.Worker.Consumers;
using Ordering_Domain.Domain.RepositoryContracts;
using Shared.IntegrationEvents.Ordering;
using Xunit;

namespace Ordering.Worker.IntegrationTests.ConsumerTests
{
    public class OrderInitiatedConsumer_Tests : TestBase.OrderStateMachineIntegrationTestBase
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;

        public OrderInitiatedConsumer_Tests(Fixtures.OrderStateMachineFixture fixture) : base(fixture)
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            // بازنویسی سرویس در ServiceProvider برای استفاده از Mock
            var serviceProvider = (ServiceProvider)DbContext.Database.GetService<IServiceProvider>();
            serviceProvider.GetService<IOrderRepository>().Returns(_mockOrderRepository.Object);
        }

        [Fact]
        public async Task Should_consume_order_initiated_message()
        {
            // Arrange
            await CleanupDatabase();
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            // Act
            await Bus.Publish(new OrderInitiated
            {
                Id = orderId,
                Customer = "TestCustomer",
                Date = now
            });

            // انتظار برای پردازش پیام
            await Task.Delay(1000);

            // Assert
            //_mockOrderRepository.Verify(r => r.SomeMethod(It.IsAny<object>()), Times.Never(),
                                        //"OrderInitiatedConsumer should not call repository methods without implementation");
        }
    }
}