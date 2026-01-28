using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Queries;

public record GetCurrentUserOrdersQuery(string State) : IQuery<OrdersListResponse>;

public record OrdersListResponse(IReadOnlyList<OrderSummaryDto> Orders);

public record OrderSummaryDto(DateTime Date, long TotalPrice);
