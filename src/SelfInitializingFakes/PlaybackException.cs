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
        /// <param name="message">Additional information about the exception.</param>
        public PlaybackException(string message)
            : base(message)
        {
        }
    }
}
