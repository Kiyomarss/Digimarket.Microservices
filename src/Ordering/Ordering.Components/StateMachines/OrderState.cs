using MassTransit;

namespace Ordering.Components.StateMachines;

public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid PaymentReference { get; set; }
    public string CurrentState { get; set; } = null!;

    public DateTime Date { get; set; }
    public string Customer { get; set; } = null!;
    public bool IsInventoryReduced { get; set; }
    public bool IsPaymentValidated { get; set; }
    public bool IsBasketRemoved { get; set; }
}