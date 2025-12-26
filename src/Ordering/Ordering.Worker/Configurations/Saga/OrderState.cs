using MassTransit;

namespace Ordering.Worker.Configurations.Saga
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public Guid PaymentReference { get; set; }
        public string CurrentState { get; set; } = null!;

        public DateTime Date { get; set; }
        public string Customer { get; set; } = null!;
        public bool IsInventoryReduced { get; set; }
        public bool IsPaymentValidated { get; set; }
        public bool IsBasketRemoved { get; set; }

        public Guid? ReminderScheduleTokenId { get; set; }
        public bool IsReminderSent { get; set; }
        
        public Guid? CancelScheduleTokenId { get; set; }

    }
}