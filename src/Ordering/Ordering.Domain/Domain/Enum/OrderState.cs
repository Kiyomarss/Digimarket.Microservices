namespace Ordering_Domain.Domain.Enum;

public sealed class OrderState
{
    // مقادیر ثابت و معتبر
    public static readonly OrderState Pending    = new(1, "Pending",    "در انتظار پرداخت");
    public static readonly OrderState Paid       = new(2, "Paid",       "پرداخت شده");
    public static readonly OrderState Processing = new(3, "Processing", "در حال پردازش");
    public static readonly OrderState Shipped    = new(4, "Shipped",    "ارسال شده");
    public static readonly OrderState Delivered  = new(5, "Delivered",  "تحویل شده");
    public static readonly OrderState Cancelled  = new(6, "Cancelled",  "لغو شده");
    public static readonly OrderState Returned   = new(7, "Returned",   "مرجوع شده");

    // خصوصیات
    public int Id { get; }
    public string Code { get; }        // نام انگلیسی (برای سریالایزیشن، مقایسه، ذخیره در دیتابیس)
    public string PersianTitle { get; } // عنوان فارسی برای نمایش در UI

    private OrderState(int id, string code, string persianTitle)
    {
        Id = id;
        Code = code;
        PersianTitle = persianTitle;
    }

    // لیست همه وضعیت‌ها (مفید برای dropdown، validation و غیره)
    private static readonly List<OrderState> _all = new()
    {
        Pending, Paid, Processing, Shipped, Delivered, Cancelled, Returned
    };

    public static IReadOnlyList<OrderState> All => _all.AsReadOnly();

    // تبدیل از کد (string) به OrderState
    public static OrderState FromCode(string code)
    {
        return _all.FirstOrDefault(s => s.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"وضعیت سفارش نامعتبر: {code}");
    }

    // تبدیل از Id (int) به OrderState
    public static OrderState FromId(int id)
    {
        return _all.FirstOrDefault(s => s.Id == id)
               ?? throw new ArgumentException($"وضعیت سفارش با شناسه {id} یافت نشد");
    }

    // تبدیل از عنوان فارسی (اختیاری - اگر نیاز داشتی)
    public static OrderState FromPersianTitle(string title)
    {
        return _all.FirstOrDefault(s => s.PersianTitle == title)
               ?? throw new ArgumentException($"عنوان فارسی نامعتبر: {title}");
    }

    // برای نمایش در UI
    public override string ToString() => PersianTitle;

    // مقایسه بر اساس Id یا Code
    public override bool Equals(object? obj)
    {
        return obj is OrderState other && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(OrderState? left, OrderState? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OrderState? left, OrderState? right)
    {
        return !Equals(left, right);
    }
}