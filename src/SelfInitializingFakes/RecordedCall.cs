namespace SelfInitializingFakes
{
    using System;

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
        public object ReturnValue { get; set; }

        /// <summary>Gets or sets the call's out and ref values. An empty array if there are none.</summary>
        /// <value>The call's out and ref values. An empty array if there are none.</value>
        public object[] OutAndRefValues { get; set; }
    }
}
