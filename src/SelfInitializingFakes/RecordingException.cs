namespace SelfInitializingFakes
{
    using System;
#if FEATURE_BINARY_SERIALIZATION
    using System.Runtime.Serialization;
#endif

    /// <summary>
    /// An exception thrown when an error is encountered while
    /// using a <see cref="SelfInitializingFake{T}"/> in recording
    /// mode.
    /// </summary>
#if FEATURE_BINARY_SERIALIZATION
    [Serializable]
#endif
    public class RecordingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingException"/> class.
        /// </summary>
        public RecordingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        public RecordingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        ///   The exception that is the cause of the current exception, or a null reference (Nothing in
        ///   Visual Basic) if no inner exception is specified.
        /// </param>
        public RecordingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if FEATURE_BINARY_SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is null or <see cref="Exception.HResult"/> is zero (0).
        /// </exception>
        protected RecordingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
