
using BuildingBlocks.CQRS;

namespace Catalog.Application.Products.Commands.ProductReservationCancelled;

public record ProductReservationCancelledCommand(IEnumerable<OrderItemDto> Items) : ICommand;

public record OrderItemDto(Guid ProductId, int Quantity);