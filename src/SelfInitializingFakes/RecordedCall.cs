namespace SelfInitializingFakes
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A saved call to a self-initialized fake.
    /// </summary>
    public class RecordedCall
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordedCall"/> class.
        /// </summary>
        /// <param name="method">The name of the method that was recorded.</param>
        /// <param name="returnValue">The return value of the method. May be <c>null</c>.</param>
        /// <param name="outAndRefValues">Any out and ref values. If none, should be a zero-length array. Items may be <c>null</c>.</param>
        internal RecordedCall(string method, object? returnValue, object?[] outAndRefValues)
        {
            this.Method = method;
            this.ReturnValue = returnValue;
            this.OutAndRefValues = outAndRefValues;
        }

#pragma warning disable CS8618 // this constructor only exists for serialization, which will populate the members
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordedCall"/> class.
        /// </summary>
        /// <remarks>
        ///  For serialization (including XML-based) only.
        /// </remarks>
        private RecordedCall()
        {
        }
#pragma warning restore CS8618

        /// <summary>Gets or sets the method that was called.</summary>
        /// <value>The string representation of the method that was called.</value>
        public string Method { get; set; }

        /// <summary>Gets or sets the call's return value.</summary>
        /// <value>The call's return value.</value>
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "The content will often be serializable, and it's up to clients to provide a serializer that can handle their weird content.")]
        public object? ReturnValue { get; set; }

        /// <summary>Gets or sets the call's out and ref values. An empty array if there are none.</summary>
        /// <value>The call's out and ref values. An empty array if there are none.</value>
        [SuppressMessage("Microsoft.Naming", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It just makes serializing it easier, and the property is not otherwise exposed to clients.")]
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "The content will often be serializable, and it's up to clients to provide a serializer that can handle their weird content.")]
        public object?[] OutAndRefValues { get; set; }
    }
}
