namespace SelfInitializingFakes
{
#if FEATURE_BINARY_SERIALIZATION
    using System;
#endif
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A saved call to a self-initialized fake.
    /// </summary>
#if FEATURE_BINARY_SERIALIZATION
    [Serializable]
#endif
    public class RecordedCall
    {
        /// <summary>Gets or sets the method that was called.</summary>
        /// <value>The string representation of the method that was called.</value>
        public string Method { get; set; }

        /// <summary>Gets or sets the call's return value.</summary>
        /// <value>The call's return value.</value>
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "The content will often be serializable, and it's up to clients to provide a serializer that can handle their weird content.")]
        public object ReturnValue { get; set; }

        /// <summary>Gets or sets the call's out and ref values. An empty array if there are none.</summary>
        /// <value>The call's out and ref values. An empty array if there are none.</value>
        [SuppressMessage("Microsoft.Naming", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It just makes serializing it easier, and the property is not otherwise exposed to clients.")]
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "The content will often be serializable, and it's up to clients to provide a serializer that can handle their weird content.")]
        public object[] OutAndRefValues { get; set; }
    }
}
