using BuildingBlocks.CQRS;
using BuildingBlocks.Services;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.RepositoryContracts;

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

        var result = await _orderRepository
                         .GetOrdersForUserAsync(userId, OrderState.FromCode(query.State), cancellationToken);

        return new OrdersListResponse(result);
    }
}