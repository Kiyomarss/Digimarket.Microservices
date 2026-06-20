
using BuildingBlocks.CQRS;

namespace Ordering.Application.Orders.Commands.CancelledAfterPayment;

public record CancelledAfterPaymentCommand(Guid Id) : ICommand;