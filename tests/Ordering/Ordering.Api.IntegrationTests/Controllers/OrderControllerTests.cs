// tests/Ordering.Api.IntegrationTests/Controllers/OrderControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Ordering.TestingInfrastructure.Fixtures;
using Xunit;

namespace Ordering.Api.IntegrationTests.Controllers;

[Collection("ApiIntegration")]
public class OrderControllerTests : IClassFixture<OrderingAppFactory>
{
    private readonly HttpClient _client;

    public OrderControllerTests(OrderingAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentUserOrders_WithValidState_Should_Return_Ok_With_Orders()
    {
        // Arrange
        var state = "WaitingForPayment";

        // Act
        var response = await _client.GetAsync($"/Order?state={state}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orders = await response.Content.ReadFromJsonAsync<List<object>>();
        orders.Should().NotBeNull();
        // اگر داده‌ای وجود داشته باشه، می‌تونی بیشتر چک کنی
    }

    [Fact]
    public async Task GetCurrentUserOrders_WithInvalidState_Should_Return_BadRequest()
    {
        // Arrange
        var invalidState = "InvalidState123";

        // Act
        var response = await _client.GetAsync($"/Order?state={invalidState}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // یا اگر ValidationBehavior داری → 400 یا 422
    }

    [Fact]
    public async Task GetCurrentUserOrders_WithEmptyState_Should_Return_All_Orders()
    {
        // Act
        var response = await _client.GetAsync("/Order");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<object>>();
        orders.Should().NotBeNull();
    }
}