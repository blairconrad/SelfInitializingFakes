namespace SelfInitializingFakes.Infrastructure
{
    using System;
#if FRAMEWORK_WEAK_TYPE_CLASS
    using System.Reflection;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// See if this type is an instance of a given open generic type.
        /// </summary>
        /// <param name="this">This type argument.</param>
        /// <param name="genericType">The generic type definition to see if this type is an instance of.</param>
        /// <returns>Type info of the type argument.</returns>
        public static bool IsInstanceOf(this Type @this, Type genericType) =>
#if FRAMEWORK_WEAK_TYPE_CLASS
            @this.GetTypeInfo().IsGenericType
#else
            @this.IsGenericType
#endif
            && @this.GetGenericTypeDefinition() == genericType;
    }
}
