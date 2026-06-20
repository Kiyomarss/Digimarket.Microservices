using BuildingBlocks.Types;

namespace Ordering_Domain.Domain.Enum;

public sealed class OrderState : TypeSafeEnum<OrderState, int>
{
    public static readonly OrderState Pending    = new(1, "Pending",    "در انتظار پرداخت");
    public static readonly OrderState Paid       = new(2, "Paid",       "پرداخت شده");
    public static readonly OrderState Processing = new(3, "Processing", "در حال پردازش");
    public static readonly OrderState CancelledAfterPayment = new(4, "CancelledAfterPayment", "لغو بعد از پرداخت");
    public static readonly OrderState Shipped    = new(5, "Shipped",    "ارسال شده");
    public static readonly OrderState Delivered  = new(6, "Delivered",  "تحویل شده");
    public static readonly OrderState Canceled   = new(7, "Canceled",  "لغو شده");
    public static readonly OrderState Returned   = new(8, "Returned",   "مرجوع شده");

    private OrderState(int id, string code, string title) : base(id, code, title) { }
}