using System.Collections.Concurrent;
using System.Reflection;

namespace BuildingBlocks.Types;

public abstract class TypeSafeEnum<T, TId> 
    where T : TypeSafeEnum<T, TId>
    where TId : IEquatable<TId>
{
    public TId Id { get; }
    public string Code { get; }
    public string Title { get; }

    protected TypeSafeEnum(TId id, string code, string title)
    {
        Id = id;
        Code = code;
        Title = title;
    }

    // کش نهایی IReadOnlyList<T>
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<T>> _cachedAll = new();

    public static IReadOnlyList<T> All
    {
        get
        {
            var type = typeof(T);
            return _cachedAll.GetOrAdd(type, _ =>
            {
                var list = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.FieldType == type)
                    .Select(f => (T)f.GetValue(null)!)
                    .ToList();

                return list.AsReadOnly(); // فقط یک بار AsReadOnly
            });
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
        => obj is TypeSafeEnum<T, TId> other && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(TypeSafeEnum<T, TId>? left, TypeSafeEnum<T, TId>? right)
        => Equals(left, right);

    public static bool operator !=(TypeSafeEnum<T, TId>? left, TypeSafeEnum<T, TId>? right)
        => !Equals(left, right);
}