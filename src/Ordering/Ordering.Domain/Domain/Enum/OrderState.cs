using BuildingBlocks.Types;

namespace Ordering_Domain.Domain.Enum;

public sealed class OrderState : TypeSafeEnum<OrderState, int>
{
    public static readonly OrderState Pending    = new(1, "Pending",    "در انتظار پرداخت");
    public static readonly OrderState Paid       = new(2, "Paid",       "پرداخت شده");
    public static readonly OrderState Processing = new(3, "Processing", "در حال پردازش");
    public static readonly OrderState Shipped    = new(4, "Shipped",    "ارسال شده");
    public static readonly OrderState Delivered  = new(5, "Delivered",  "تحویل شده");
    public static readonly OrderState Cancelled  = new(6, "Cancelled",  "لغو شده");
    public static readonly OrderState Returned   = new(7, "Returned",   "مرجوع شده");

    private OrderState(int id, string code, string title) : base(id, code, title) { }
}