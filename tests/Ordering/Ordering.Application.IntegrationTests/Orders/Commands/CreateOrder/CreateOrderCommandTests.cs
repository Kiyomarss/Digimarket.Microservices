using BuildingBlocks.Exceptions.Application;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using ProductGrpc;
using Shared;

namespace Ordering.Application.IntegrationTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests : OrderingAppTestBase
{
    public CreateOrderCommandHandlerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Handle_Should_Create_Order_With_Items()
    {
        await ResetDatabase();

        var command = new CreateOrderCommand
        {
            Items =
            [
                new() { ProductId = TestGuids.Guid1, Quantity = 2 },
                new() { ProductId = TestGuids.Guid2, Quantity = 1 }
            ]
        };

        // Act
        var orderId = await Sender.Send(command);

        // Assert
        var order = await DbContext.Orders
                                  .Include(o => o.Items)
                                  .FirstAsync(o => o.Id == orderId);

        order.Items.Should().HaveCount(2);

        order.Items.Sum(i => i.Quantity).Should().Be(3);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Products_Not_Found()
    {
        await ResetDatabase();
        
        Fixture.ProductServiceMock
               .Setup(x => x.GetProductsByIdsAsync(
                                                   It.IsAny<IEnumerable<string>>(),
                                                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new GetProductsResponse());


        var command = new CreateOrderCommand
        {
            Items =
            [
                new() { ProductId = Guid.NewGuid().ToString(), Quantity = 1 }
            ]
        };

        var act = () => Sender.Send(command);

        await act.Should().ThrowAsync<ExternalServiceException>()
                 .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_Should_Map_Product_Prices_Correctly()
    {
        await ResetDatabase();
        
        Fixture.ProductServiceMock
               .Setup(x => x.GetProductsByIdsAsync(
                                                   It.IsAny<IEnumerable<string>>(),
                                                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(DefaultProducts);


        var command = new CreateOrderCommand
        {
            Items =
            [
                new() { ProductId = TestGuids.Guid1, Quantity = 2 }
            ]
        };

        var orderId = await Sender.Send(command);

        var order = await DbContext.Orders
                                  .Include(o => o.Items)
                                  .FirstAsync(o => o.Id == orderId);

        var item = order.Items.Single();

        item.Quantity.Should().Be(2);
        item.Price.Should().BeGreaterThan(0);

        return;

        GetProductsResponse DefaultProducts()
        {
            var response = new GetProductsResponse();

            response.Products.Add(new ProductInfo
            {
                ProductId = TestGuids.Guid1,
                Price = 1500
            });

            return response;
        }
    }
}