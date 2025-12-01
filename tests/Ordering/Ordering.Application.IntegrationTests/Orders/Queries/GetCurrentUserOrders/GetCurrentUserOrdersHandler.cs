using AutoFixture;
using FluentAssertions;
using Ordering_Domain.Domain.Entities;
using Ordering.Application.Orders.Queries;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;

namespace Ordering.Application.IntegrationTests.Orders.Queries.GetCurrentUserOrders;

public class GetCurrentUserOrdersHandler : OrderingAppTestBase
{
    public GetCurrentUserOrdersHandler(OrderingAppFactory fixture) 
        : base(fixture) { }
    
    [Fact]
    public async Task Handle_Should_Return_Only_Shipped_Orders()
    {
        // Arrange
        await CleanupDatabase();

        DbContext.Orders.AddRange(
                                  OrderBuilder.Processing(),
                                  OrderBuilder.Shipped(),
                                  OrderBuilder.Pending()
                                 );
        await DbContext.SaveChangesAsync();

        var query = new GetCurrentUserOrdersQuery("Shipped");

        // Act & Assert
        var result = await Sender.Send(query);
        result.Orders.Should().HaveCount(1);
    }}