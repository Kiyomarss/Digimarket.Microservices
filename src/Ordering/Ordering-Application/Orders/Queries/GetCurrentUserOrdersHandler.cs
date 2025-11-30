using BuildingBlocks.CQRS;
using BuildingBlocks.Services;
using Ordering_Domain.Domain.RepositoryContracts;

namespace Ordering.Application.Orders.Queries;

public class GetCurrentUserOrdersHandler 
    : IQueryHandler<GetCurrentUserOrdersQuery, OrdersListResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserOrdersHandler(IOrderRepository orderRepository, ICurrentUserService currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<OrdersListResponse> Handle(
        GetCurrentUserOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetRequiredUserId();

        var orders = await _orderRepository
                         .GetOrdersForUserAsync(userId, query.State, cancellationToken);

        var result = orders.Select(order => new OrderSummaryDto(order.Date,
                                                         order.Items.Sum(i => i.Price * i.Quantity)
                                                        )).ToList();

        return new OrdersListResponse(result);
    }
}