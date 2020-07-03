namespace SelfInitializingFakes.Infrastructure
{
    using System;

    /// <summary>
    /// Converts unserializable types to simpler types while recording, and reverses the transformation during.
    /// </summary>
    internal interface ITypeConverter
    {
        /// <summary>
        /// Potentially converts an unserializable object to a more serializable form.
        /// </summary>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be assigned to a simpler representation of <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        bool ConvertForRecording(object? input, ITypeConverter mainConverter, out object? output);

        /// <summary>
        /// Potentially converts the serializable form of an object back to its unserializable form.
        /// </summary>
        /// <param name="deserializedType">The desired deserialized type.</param>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be reconstituted from its simpler representation as <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        bool ConvertForPlayback(Type deserializedType, object? input, ITypeConverter mainConverter, out object? output);
    }
}