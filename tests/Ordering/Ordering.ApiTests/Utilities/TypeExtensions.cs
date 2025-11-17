using System;

public static class TypeExtensions
{
    public static bool IsAssignableFromGeneric(this Type genericType, Type givenType)
    {
        if (!genericType.IsGenericTypeDefinition) return false;

        while (givenType != null && givenType != typeof(object))
        {
            var cur = givenType.IsGenericType ? givenType.GetGenericTypeDefinition() : givenType;
            if (genericType == cur) return true;
            givenType = givenType.BaseType;
        }
        return false;
    }
}