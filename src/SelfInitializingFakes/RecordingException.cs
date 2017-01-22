namespace SelfInitializingFakes
{
    using System;

    /// <summary>
    /// An exception thrown when an error is encountered while
    /// using a <see cref="SelfInitializingFake{T}"/> in recording
    /// mode.
    /// </summary>
    public class RecordingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        public RecordingException(string message)
            : base(message)
        {
        }
    }
}
