namespace SelfInitializingFakes
{
    using System.Reflection;

    /// <summary>
    /// A saved call to a self-initialized fake.
    /// </summary>
    public interface ICallData
    {
        /// <summary>Gets the method that was called.</summary>
        /// <value>The method that was called.</value>
        MethodInfo Method { get; }

        /// <summary>Gets the call's return value.</summary>
        /// <value>The call's return value.</value>
        object ReturnValue { get; }
    }
}
