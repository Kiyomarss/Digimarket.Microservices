using System.Collections.Concurrent;

namespace BuildingBlocks.Types;

/// <summary>
/// پایه ژنریک برای پیاده‌سازی Type-Safe Enum Pattern
/// </summary>
/// <typeparam name="T">نوع خود enum (مثل OrderState)</typeparam>
/// <typeparam name="TId">نوع شناسه (معمولاً int یا string)</typeparam>
public abstract class TypeSafeEnum<T, TId> 
    where T : TypeSafeEnum<T, TId>
    where TId : IEquatable<TId>
{
    public TId Id { get; }
    public string Code { get; }
    public string Title { get; } // مثلاً عنوان فارسی یا انگلیسی برای نمایش

    protected TypeSafeEnum(TId id, string code, string title)
    {
        Id = id;
        Code = code;
        Title = title;
    }

    // کش برای تمام مقادیر (thread-safe)
    private static readonly ConcurrentDictionary<Type, IEnumerable<T>> _allValues = new();

    public static IReadOnlyList<T> All
    {
        get
        {
            var type = typeof(T);
            return _allValues.GetOrAdd(type, _ => 
                type.GetFields(System.Reflection.BindingFlags.Public | 
                               System.Reflection.BindingFlags.Static | 
                               System.Reflection.BindingFlags.FlattenHierarchy)
                    .Where(f => f.FieldType == type)
                    .Select(f => (T)f.GetValue(null)!)
                    .ToList()
            ).ToList().AsReadOnly();
        }
    }

    public static T FromCode(string code)
    {
        return All.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"کد نامعتبر: {code}");
    }

    public static bool TryFromCode(string code, out T? value)
    {
        value = All.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return value != null;
    }

    public static T FromId(TId id)
    {
        return All.FirstOrDefault(x => x.Id.Equals(id))
               ?? throw new ArgumentException($"شناسه نامعتبر: {id}");
    }

    public override string ToString() => Title;

    public override bool Equals(object? obj)
    {
        return obj is TypeSafeEnum<T, TId> other && Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(TypeSafeEnum<T, TId>? left, TypeSafeEnum<T, TId>? right)
        => Equals(left, right);

    public static bool operator !=(TypeSafeEnum<T, TId>? left, TypeSafeEnum<T, TId>? right)
        => !Equals(left, right);
}