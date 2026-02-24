using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.Api.Contracts;
using Ordering.Application.Orders.Queries;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;

namespace Ordering.Api.IntegrationTests.Controllers;

[Collection("ApiIntegration")]
public class OrderControllerTests : OrderingAppTestBase
{
    private readonly HttpClient _client;

    public OrderControllerTests(OrderingAppFactory fixture) : base(fixture)
    {
        _client = fixture.CreateClient();
    }


    [Fact]
    public async Task GetCurrentUserOrders_WithState_ShouldReturn_Orders()
    {
        await ResetDatabase();

        // Arrange
        DbContext.Orders.AddRange(
                                  new OrderBuilder()
                                      .WithState(OrderState.Pending)
                                      .WithItems((1, 200000))
                                      .Build());

        await DbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"{ApiEndpoints.Orders.GetCurrentUserOrders}?state={OrderState.Pending.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<OrdersListResponse>();

        result.Should().NotBeNull();
        result!.Orders.Should().HaveCount(1);
    }
}