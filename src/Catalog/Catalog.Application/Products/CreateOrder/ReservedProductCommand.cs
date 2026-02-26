
using BuildingBlocks.CQRS;

namespace Catalog.Application.Products.CreateOrder;

public record ReserveProductsCommand(
    List<OrderItemDto> Items
) : ICommand<ReserveProductsResponse>;

public record OrderItemDto(Guid ProductId, int Quantity);

public record ReservedProductCommand(
    Guid ProductId,
    long Price
);

public record ReserveProductsResponse(
    IEnumerable<ReservedProductCommand> Products
);