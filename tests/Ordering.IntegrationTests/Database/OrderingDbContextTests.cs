using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.Entities;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.IntegrationTests.Config;

namespace Ordering.IntegrationTests.Database;

public class OrderingDbContextTests : IClassFixture<LocalPostgresFixture>
{
    private readonly LocalPostgresFixture _fixture;

    public OrderingDbContextTests(LocalPostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private OrderingDbContext CreateDbContext()
        => _fixture.CreateDbContext();

    [Fact]
    public async Task Should_Create_Order_With_Items_And_Correct_Relations()
    {
        var db = CreateDbContext();

        var order = new Order
        {
            Customer = "kiomars",
            State = "Pending",
            Items =
            {
                new OrderItem(
                    productId: Guid.NewGuid(),
                    productName: "Product A",
                    quantity: 2,
                    price: 5000
                )
            }
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var savedOrder = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        savedOrder.Should().NotBeNull();
        savedOrder!.Customer.Should().Be("kiomars");
        savedOrder.State.Should().Be("Pending");

        savedOrder.Items.Should().HaveCount(1);
        var item = savedOrder.Items.First();

        item.ProductName.Should().Be("Product A");
        item.Quantity.Should().Be(2);
        item.Price.Should().Be(5000);

        savedOrder.TotalPrice.Should().Be(10000);
    }

    [Fact]
    public void Should_Contain_Outbox_And_Inbox_Tables()
    {
        var db = CreateDbContext();
        var model = db.Model;

        model.GetEntityTypes().Any(e => e.Name.Contains("InboxState")).Should().BeTrue();
        model.GetEntityTypes().Any(e => e.Name.Contains("OutboxMessage")).Should().BeTrue();
        model.GetEntityTypes().Any(e => e.Name.Contains("OutboxState")).Should().BeTrue();
    }

    [Fact]
    public void Should_Apply_Configurations_From_Assembly()
    {
        var db = CreateDbContext();
        var entity = db.Model.FindEntityType(typeof(Order));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("orders");

        entity.GetProperties().Select(p => p.Name)
            .Should().Contain(new[] { "Customer", "State", "Date", "Id" });
    }

    [Fact]
    public async Task Deleting_Order_Should_Delete_Its_Items()
    {
        var db = CreateDbContext();

        var order = new Order
        {
            Customer = "kiomars",
            State = "Pending",
            Items =
            {
                new OrderItem(Guid.NewGuid(), "Test", 1, 1000)
            }
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Act
        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        var items = await db.OrderItems.ToListAsync();

        items.Should().BeEmpty();   // Cascade delete ✅
    }

    [Fact]
    public async Task Should_Fail_When_Customer_Is_Null()
    {
        var db = CreateDbContext();

        var order = new Order
        {
            Customer = null!,     // ✅ PostgreSQL خطا خواهد داد
            State = "Pending"
        };

        db.Orders.Add(order);

        Func<Task> act = async () => await db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}