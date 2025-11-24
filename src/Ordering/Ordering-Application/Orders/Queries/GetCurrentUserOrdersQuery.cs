using BuildingBlocks.CQRS;

namespace Ordering.Core.Orders.Queries;

public record GetCurrentUserOrdersQuery(string State) : IQuery<OrdersListResponse>;

public record OrdersListResponse(List<OrderSummaryDto> Orders);

public record OrderSummaryDto(DateTime Date, long TotalPrice);
