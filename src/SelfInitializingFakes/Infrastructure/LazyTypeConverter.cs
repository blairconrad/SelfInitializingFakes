namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Converts <see cref="System.Lazy{T}" /> types to simpler types for serialization, and back again.
    /// </summary>
    internal class LazyTypeConverter : ITypeConverter
    {
        private static readonly MethodInfo CreateLazyGenericDefinition =
           typeof(LazyTypeConverter).GetMethod(nameof(CreateLazy), BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Potentially converts an unserializable object to a more serializable form.
        /// </summary>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be assigned to a simpler representation of <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        public bool ConvertForRecording(object? input, ITypeConverter mainConverter, out object? output)
        {
            output = null;
            if (input is null)
            {
                return false;
            }

            var inputType = input.GetType();
            if (inputType.IsInstanceOf(typeof(Lazy<>)))
            {
                output = inputType.GetProperty("Value").GetGetMethod().Invoke(input, Type.EmptyTypes);
                if (mainConverter.ConvertForRecording(output, mainConverter, out object? furtherConvertedOutput))
                {
                    output = furtherConvertedOutput;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Potentially converts the serializable form of an object back to its unserializable form.
        /// </summary>
        /// <param name="deserializedType">The desired deserialized type.</param>
        /// <param name="input">An input object.</param>
        /// <param name="mainConverter">A comprehensive converter that may be used to further convert the output, if required.</param>
        /// <param name="output">An output object. Will be reconstituted from its simpler representation as <paramref name="input"/>, if this converter knows how.</param>
        /// <returns><c>true</c> if the conversion happened, otherwise <c>false</c>. Good for building a chain of responsibility.</returns>
        public bool ConvertForPlayback(Type deserializedType, object? input, ITypeConverter mainConverter, out object? output)
        {
            if (deserializedType.IsInstanceOf(typeof(Lazy<>)))
            {
                var typeOfLazyResult = deserializedType.GetGenericArguments()[0];

                if (input is null || input.GetType() == typeOfLazyResult)
                {
                    var method = CreateLazyGenericDefinition.MakeGenericMethod(typeOfLazyResult);
                    output = method.Invoke(null, new object?[] { input });
                    return true;
                }

                if (mainConverter.ConvertForPlayback(typeOfLazyResult, input, mainConverter, out object? convertedInput))
                {
                    var method = CreateLazyGenericDefinition.MakeGenericMethod(typeOfLazyResult);
                    output = method.Invoke(null, new object?[] { convertedInput });
                    return true;
                }
            }

            output = null;
            return false;
        }

        private static Lazy<T> CreateLazy<T>(T value) => new Lazy<T>(() => value);
    }
}
