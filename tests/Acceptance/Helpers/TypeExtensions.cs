namespace SelfInitializingFakes.Tests.Acceptance.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if FRAMEWORK_WEAK_TYPE_CLASS
    using System.Reflection;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Find all concrete subtypes of the given type from the same assembly.
        /// </summary>
        /// <param name="this">This type argument.</param>
        /// <returns>A list of all concrete subtypes.</returns>
        public static IEnumerable<Type> GetConcreteSubTypesInAssembly(this Type @this) =>
#if FRAMEWORK_WEAK_TYPE_CLASS
            typeof(FileBasedRecordedCallRepository).GetTypeInfo().Assembly.GetTypes()
               .Where(t => @this.GetTypeInfo().IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract);
#else
            typeof(FileBasedRecordedCallRepository).Assembly.GetTypes()
               .Where(t => @this.IsAssignableFrom(t) && !t.IsAbstract);
#endif
    }
}