namespace Ordering.Worker.StateMachines.Events;

public record PaymentCompleted(Guid CorrelationId);