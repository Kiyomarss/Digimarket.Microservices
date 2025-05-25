using MassTransit;

namespace Ordering.Components.StateMachines;

public class OrderState :
    SagaStateMachineInstance
{
    public string CurrentState { get; set; } = null!;

    public DateTime Date { get; set; }
    public string Customer { get; set; } = null!;
    public Guid CorrelationId { get; set; }
}