namespace SelfInitializingFakes
{
    using System.Reflection;

    /// <summary>
    /// A saved call to a self-initialized fake.
    /// </summary>
    public class CallData
    {
        /// <summary>Gets or sets the method that was called.</summary>
        /// <value>The method that was called.</value>
        public MethodInfo Method { get; set; }

        /// <summary>Gets or sets the call's return value.</summary>
        /// <value>The call's return value.</value>
        public object ReturnValue { get; set; }

        /// <summary>Gets or sets the call's out and ref values. An empty array if there are none.</summary>
        /// <value>The call's out and ref values. An empty array if there are none.</value>
        public object[] OutAndRefValues { get; set; }
    }
}
