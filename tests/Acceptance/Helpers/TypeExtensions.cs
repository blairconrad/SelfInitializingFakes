namespace SelfInitializingFakes.Tests.Acceptance.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            typeof(FileBasedRecordedCallRepository).Assembly.GetTypes()
               .Where(t => @this.IsAssignableFrom(t) && !t.IsAbstract);
    }
}
