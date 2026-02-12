using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Queries;
using Ordering.TestingInfrastructure.Fixtures;

namespace Ordering.Api.IntegrationTests.Controllers;

[Collection("ApiIntegration")]
public class OrderControllerTests : IClassFixture<OrderingAppFactory>
{
    private readonly HttpClient _client;
    private readonly OrderingAppFactory _factory;

    public OrderControllerTests(OrderingAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("Pending", 200000)]
    public async Task GetCurrentUserOrders_WithState_ShouldReturn_CorrectTotal(string state, long expectedTotal)
    {
        var dbContext = _factory.DbContext;

        dbContext.Orders.AddRange(
                                  new OrderBuilder().WithItems((1, 200000)).Build()
                                 );
        await dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/Order/GetCurrentUserOrders?state={state}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<OrdersListResponse>();

        // Assert
        result.Should().NotBeNull();
        result.Orders[0].TotalPrice.Should().Be(expectedTotal);
    }
}