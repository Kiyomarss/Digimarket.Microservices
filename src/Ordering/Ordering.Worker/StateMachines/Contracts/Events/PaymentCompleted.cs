namespace Ordering.Worker.StateMachines.Contracts.Events;

public record PaymentCompleted(Guid CorrelationId);