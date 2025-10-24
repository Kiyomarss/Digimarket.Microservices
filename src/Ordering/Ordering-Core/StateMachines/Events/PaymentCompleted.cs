namespace Ordering.Core.StateMachines.Events;

public record PaymentCompleted(Guid CorrelationId);