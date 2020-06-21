namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Chains other <see cref="ITypeConverter"/>s together.
    /// </summary>
    internal class CompoundTypeConverter : ITypeConverter
    {
        private readonly ITypeConverter first;
        private readonly ITypeConverter second;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundTypeConverter"/> class.
        /// </summary>
        /// <param name="first">The first converter to try. If it can't convert the input, the second will be tried.</param>
        /// <param name="second">The second converter to try, if the first was unable.</param>
        public CompoundTypeConverter(ITypeConverter first, ITypeConverter second)
        {
            this.first = first;
            this.second = second;
        }

        /// <summary>
        /// Potentially converts an unserializable object to a more serializable form.
        /// </summary>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be assigned to a simpler representation of <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        public bool ConvertForRecording(object? input, ITypeConverter mainConverter, out object? output) =>
            this.first.ConvertForRecording(input, mainConverter, out output) ||
            this.second.ConvertForRecording(input, mainConverter, out output);

        /// <summary>
        /// Potentially converts the serializable form of an object back to its unserializable form.
        /// </summary>
        /// <param name="deserializedType">The desired deserialized type.</param>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be reconstituted from its simpler representation as <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        public bool ConvertForPlayback(Type deserializedType, object? input, ITypeConverter mainConverter, out object? output) =>
            this.first.ConvertForPlayback(deserializedType, input, mainConverter, out output) ||
            this.second.ConvertForPlayback(deserializedType, input, mainConverter, out output);
    }
}
