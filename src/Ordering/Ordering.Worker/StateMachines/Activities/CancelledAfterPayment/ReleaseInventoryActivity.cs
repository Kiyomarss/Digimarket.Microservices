using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.IntegrationEvents.Order;
using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;

namespace Ordering.Worker.StateMachines.Activities.CancelledAfterPayment;

public class ReleaseInventoryActivity :
    BaseActivity<OrderState, OrderCancelledAfterPayment>
{
    public override async Task Execute(
        BehaviorContext<OrderState, OrderCancelledAfterPayment> context,
        IBehavior<OrderState, OrderCancelledAfterPayment> next)
    {
        var items = context.Message.Items.Select(x => new ProductReservationCancelled.ProductItemsDto(x.ProductId, x.Quantity));
        await context.Publish(new ProductReservationCancelled(items));

        await next.Execute(context);
    }
}