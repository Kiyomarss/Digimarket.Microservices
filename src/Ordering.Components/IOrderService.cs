namespace Ordering.Components;

public interface IOrderService
{
    Task<Order> SubmitOrders(List<OrderItem> orderItems);
}