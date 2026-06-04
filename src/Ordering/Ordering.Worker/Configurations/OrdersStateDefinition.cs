using MassTransit;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.DbContext;

namespace Ordering.Worker.Configurations;

public class OrdersStateDefinition :
    SagaDefinition<OrderState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<OrderState> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<OrdersSagaDbContext>(context);
    }
}

/*UseMessageRetry:
مشخص می‌کند اگر در حین پردازش یک پیام خطایی رخ داد
(مثلاً دیتابیس قفل بود یا موقتاً قطع شد)،
چند بار و با چه فواصل زمانی (بر حسب میلی‌ثانیه) تلاش مجدد (Retry) انجام شود.

UseEntityFrameworkOutbox:
الگوی Transactional Outbox را برای این Saga فعال می‌کند.
به این معنی که تغییراتِ وضعیت Saga (در دیتابیس) و پیام‌های جدیدی که این Saga تولید می‌کند،
همگی در یک تراکنش (Transaction) واحد ذخیره می‌شوند.
اگر ذخیره در دیتابیس موفق بود،
پیام‌ها به صف Message Broker (مثل RabbitMQ) ارسال می‌شوند.
این کار از گم شدن پیام‌ها یا ارسال پیام‌های تکراری جلوگیری می‌کند.*/