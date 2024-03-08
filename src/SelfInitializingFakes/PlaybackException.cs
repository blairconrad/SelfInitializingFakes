namespace SelfInitializingFakes
{
    using System;

    /// <summary>
    /// An exception thrown when an error is encountered while
    /// using a <see cref="SelfInitializingFake{T}"/> in playback
    /// mode.
    /// </summary>
    public class PlaybackException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackException"/> class.
        /// </summary>
        public PlaybackException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        public PlaybackException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">
        ///   The exception that is the cause of the current exception, or a null reference
        ///   (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public PlaybackException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
