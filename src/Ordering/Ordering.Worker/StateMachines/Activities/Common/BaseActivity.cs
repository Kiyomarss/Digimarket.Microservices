using MassTransit;
using Ordering.Worker.Configurations.Saga;

namespace Ordering.Worker.StateMachines.Activities.Common
{
    public abstract class BaseActivity<TSaga, TEvent> :
        IStateMachineActivity<TSaga, TEvent>
        where TSaga : OrderState
        where TEvent : class
    {
        public abstract Task Execute(
            BehaviorContext<TSaga, TEvent> context,
            IBehavior<TSaga, TEvent> next);

        public virtual Task Faulted<TException>(
            BehaviorExceptionContext<TSaga, TEvent, TException> context,
            IBehavior<TSaga, TEvent> next)
            where TException : Exception
        {
            // Pass-through fault handling
            return next.Faulted(context);
        }

        public virtual void Probe(ProbeContext context)
        {
            context.CreateScope(GetType().Name);
        }

        public virtual void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
