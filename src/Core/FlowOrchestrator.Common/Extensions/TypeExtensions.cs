using System.Reflection;

namespace FlowOrchestrator.Common.Extensions;

/// <summary>
/// Provides extension methods for types.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether a type is a nullable type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a nullable type; otherwise, false.</returns>
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets the underlying type of a nullable type.
    /// </summary>
    /// <param name="type">The nullable type.</param>
    /// <returns>The underlying type of the nullable type.</returns>
    public static Type GetUnderlyingType(this Type type)
    {
        return type.IsNullable() ? Nullable.GetUnderlyingType(type)! : type;
    }

    /// <summary>
    /// Determines whether a type implements a specified interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="interfaceType">The interface type.</param>
    /// <returns>True if the type implements the interface; otherwise, false.</returns>
    public static bool ImplementsInterface(this Type type, Type interfaceType)
    {
        return type.GetInterfaces().Any(i => i == interfaceType || (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));
    }

    /// <summary>
    /// Determines whether a type is a subclass of a specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="baseType">The base type.</param>
    /// <returns>True if the type is a subclass of the base type; otherwise, false.</returns>
    public static bool IsSubclassOf(this Type type, Type baseType)
    {
        return type.IsSubclassOf(baseType) || (baseType.IsGenericType && type.IsSubclassOfGeneric(baseType));
    }

    /// <summary>
    /// Determines whether a type is a subclass of a specified generic type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="genericType">The generic type.</param>
    /// <returns>True if the type is a subclass of the generic type; otherwise, false.</returns>
    public static bool IsSubclassOfGeneric(this Type type, Type genericType)
    {
        if (type == null || genericType == null || !genericType.IsGenericType)
        {
            return false;
        }

        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition())
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets all properties of a type that have a specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="type">The type.</param>
    /// <returns>All properties of the type that have the specified attribute.</returns>
    public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type.GetProperties().Where(p => p.GetCustomAttributes(typeof(TAttribute), true).Any());
    }

    /// <summary>
    /// Gets all methods of a type that have a specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="type">The type.</param>
    /// <returns>All methods of the type that have the specified attribute.</returns>
    public static IEnumerable<MethodInfo> GetMethodsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type.GetMethods().Where(m => m.GetCustomAttributes(typeof(TAttribute), true).Any());
    }
}
