namespace Ordering.Worker.StateMachines.Events;

public record OrderStatusChanged 
{
    public Guid Id { get; set; }

    public string OrderState { get; set; }
}