using BuildingBlocks.Types;

namespace Shared.Tests.Types;

public sealed class Status : TypeSafeEnum<Status, int>
{
    public static readonly Status Active   = new(1, "ACTIVE",   "فعال");
    public static readonly Status Inactive = new(2, "INACTIVE", "غیرفعال");
    public static readonly Status Pending  = new(3, "PENDING",  "در انتظار");

    private Status(int id, string code, string title) : base(id, code, title) { }
}

public sealed class Role : TypeSafeEnum<Role, string>
{
    public static readonly Role Admin    = new("admin",    "ADMIN",    "مدیر");
    public static readonly Role User     = new("user",     "USER",     "کاربر");
    public static readonly Role Guest    = new("guest",    "GUEST",    "مهمان");

    private Role(string id, string code, string title) : base(id, code, title) { }
}