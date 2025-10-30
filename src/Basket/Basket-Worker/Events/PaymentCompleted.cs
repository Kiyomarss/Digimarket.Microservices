namespace Basket.Worker.Events;

public record PaymentCompleted(Guid Id, Guid TransactionId);
