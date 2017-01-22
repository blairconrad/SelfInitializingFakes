namespace SelfInitializingFakes
{
    using System.Collections.Generic;

    /// <summary>
    /// Saves and loads recorded calls made to a service.
    /// </summary>
    public interface ICallDataRepository
    {
        /// <summary>
        /// Saves recorded calls for later use.
        /// </summary>
        /// <param name="calls">The recorded calls to save.</param>
        void Save(IEnumerable<CallData> calls);

        /// <summary>
        /// Loads and returns saved calls.
        /// </summary>
        /// <returns>The saved calls, or <c>null</c> if there are none.</returns>
        IEnumerable<CallData> Load();
    }
}
