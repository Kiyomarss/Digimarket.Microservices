using BuildingBlocks.Common.Events;
using MediatR;
using Ordering_Domain.Domain.Entities;

namespace Ordering_Domain.Domain.Events;

public record OrderCreatedDomainEvent(
    Guid Id,
    Guid UserId,
    DateTime Date,
    string Customer,
    IReadOnlyList<OrderItem> Items
    ) : IDomainEvent, INotification;