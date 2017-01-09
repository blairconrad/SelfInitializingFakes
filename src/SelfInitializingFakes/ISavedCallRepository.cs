namespace SelfInitializingFakes
{
    using System.Collections.Generic;

    /// <summary>
    /// Saves and loads recorded calls made to a service.
    /// </summary>
    public interface ISavedCallRepository
    {
        /// <summary>
        /// Loads and returns saved calls.
        /// </summary>
        /// <returns>The saved calls, or <c>null</c> if there are none.</returns>
        IEnumerable<ISavedCall> LoadCalls();
    }
}
