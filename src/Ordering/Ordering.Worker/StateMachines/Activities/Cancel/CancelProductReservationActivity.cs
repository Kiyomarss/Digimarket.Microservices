using BuildingBlocks.IntegrationEvents;
using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines.Activities.Common;
using Ordering.Worker.StateMachines.Events;

namespace Ordering.Worker.StateMachines.Activities.Cancel;

public class CancelProductReservationActivity :
    BaseActivity<OrderState, CancelOrder>
{
    public override async Task Execute(
        BehaviorContext<OrderState, CancelOrder> context,
        IBehavior<OrderState, CancelOrder> next)
    {
        var items = context.Message.Items.Select(x => new ProductReservationCancelled.ProductItemsDto(x.ProductId, x.Quantity));
        await Task.WhenAll(
                           context.Publish(new OrderCanceled(context.Message.Id)),
                           context.Publish(new ProductReservationCancelled(items))
                          );
        await next.Execute(context);
    }
}