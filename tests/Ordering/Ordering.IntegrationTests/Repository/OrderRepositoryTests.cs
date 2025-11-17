using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.Entities;
using Ordering_Infrastructure.Data.Persistence;
using Ordering_Infrastructure.Repositories;
using Ordering.IntegrationTests.Config;

namespace Ordering.IntegrationTests.Repository;

public class OrderRepositoryTests : IClassFixture<LocalPostgresFixture>
{
    private readonly LocalPostgresFixture _fixture;

    public OrderRepositoryTests(LocalPostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private OrderRepository CreateRepository()
    {
        // همان DbContext استفاده شود تا تغییرات ذخیره شوند
        var db = _fixture.CreateDbContext();
        return new OrderRepository(db);
    }

    [Fact]
    public async Task Deleting_Order_Should_Remove_Its_Items_Cascade()
    {
        var db = _fixture.CreateDbContext();
        var repo = new OrderRepository(db);

        var order = new Order
        {
            Customer = "kiomars",
            State = "Pending",
            Items =
            {
                new OrderItem(Guid.NewGuid(), "Product X", 1, 1500)
            }
        };

        await repo.AddOrder(order);
        await db.SaveChangesAsync();

        // Act
        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        var items = await db.OrderItems.Where(x => x.OrderId == order.Id).ToListAsync();
        items.Should().BeEmpty(); // Cascade delete ✅
    }

    [Fact]
    public async Task AddOrder_Should_Add_Order_To_Db()
    {
        var db = _fixture.CreateDbContext();
        var repo = new OrderRepository(db); // همان DbContext

        var order = new Order
        {
            Customer = "kiomars",
            State = "Pending",
            Items =
            {
                new OrderItem(Guid.NewGuid(), "Test Product", 2, 5000)
            }
        };

        await repo.AddOrder(order);
        await db.SaveChangesAsync(); // داده‌ها ذخیره می‌شوند

        var savedOrder = await db.Orders
                                 .Include(o => o.Items)
                                 .FirstOrDefaultAsync(o => o.Id == order.Id);

        savedOrder.Should().NotBeNull();
        savedOrder!.Items.Should().HaveCount(1);
        savedOrder.Customer.Should().Be("kiomars");
    }

    [Fact]
    public async Task FindOrderById_Should_Return_Order_With_Items()
    {
        var db = _fixture.CreateDbContext();
        var repo = new OrderRepository(db);

        var order = new Order
        {
            Customer = "kiomars",
            State = "Pending",
            Items =
            {
                new OrderItem(Guid.NewGuid(), "Test Product", 1, 2000)
            }
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(); // داده‌ها ذخیره می‌شوند

        var result = await repo.FindOrderById(order.Id);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Customer.Should().Be("kiomars");
    }

    [Fact]
    public async Task Adding_Order_With_Null_Customer_Should_Fail()
    {
        var db = _fixture.CreateDbContext();
        var repo = new OrderRepository(db);

        var order = new Order
        {
            Customer = null!, // باید خطا بدهد
            State = "Pending"
        };

        await repo.AddOrder(order);

        Func<Task> act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
    
    [Fact]
    public async Task SaveChangesAsync_Should_Save_Data()
    {
        var db = _fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(db);

        var order = new Order
        {
            Customer = "Test",
            State = "Pending"
        };

        db.Orders.Add(order);
    
        var result = await unitOfWork.SaveChangesAsync();
    
        result.Should().Be(1); // یک ردیف تغییر کرده
        (await db.Orders.FirstOrDefaultAsync(o => o.Id == order.Id)).Should().NotBeNull();
    }
}