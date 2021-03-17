namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Extension methods for exceptions.
    /// </summary>
    internal static class ExceptionExtensions
    {
        private static readonly Action<Exception> PreserveStackTrace = CreatePreserveStackTrace();

        /// <summary>
        /// Re-throws an exception, trying to preserve its stack trace.
        /// </summary>
        /// <param name="exception">The exception to rethrow.</param>
        public static void Rethrow(this Exception exception)
        {
            try
            {
                PreserveStackTrace(exception);
            }
#pragma warning disable CA1031 // We're rethrowing an exception. If preserving the stack trace fails, there's nothing we can do
            catch
#pragma warning restore CA1031 // We're rethrowing an exception. If preserving the stack trace fails, there's nothing we can do
            {
            }

            throw exception;
        }

        private static Action<Exception> CreatePreserveStackTrace()
        {
            var method = typeof(Exception).GetMethod(
                "InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            return (Action<Exception>)Delegate.CreateDelegate(typeof(Action<Exception>), null, method);
        }
    }
}
