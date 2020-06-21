namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Converts <see cref="Task{T}" /> types to simpler types for serialization, and back again.
    /// </summary>
    internal class TaskTypeConverter : ITypeConverter
    {
        private static readonly MethodInfo CreateTaskGenericDefinition =
           typeof(TaskTypeConverter).GetMethod(nameof(CreateTask), BindingFlags.Static | BindingFlags.NonPublic);

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
            if (inputType.IsInstanceOf(typeof(Task<>)))
            {
                output = inputType.GetProperty("Result").GetGetMethod().Invoke(input, Type.EmptyTypes);
                if (mainConverter.ConvertForRecording(output, mainConverter, out object? furtherConvertedOutput))
                {
                    output = furtherConvertedOutput;
                }

                return true;
            }
            else if (inputType == typeof(Task))
            {
                output = "{void Task}";
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
            if (deserializedType.IsInstanceOf(typeof(Task<>)))
            {
                var typeOfTaskResult = deserializedType.GetGenericArguments()[0];

                if (input is null || input.GetType() == typeOfTaskResult)
                {
                    var method = CreateTaskGenericDefinition.MakeGenericMethod(typeOfTaskResult);
                    output = method.Invoke(null, new object?[] { input });
                    return true;
                }

                if (mainConverter.ConvertForPlayback(typeOfTaskResult, input, mainConverter, out object? convertedInput))
                {
                    var method = CreateTaskGenericDefinition.MakeGenericMethod(typeOfTaskResult);
                    output = method.Invoke(null, new object?[] { convertedInput });
                    return true;
                }
            }
            else if (deserializedType == typeof(Task) && "{void Task}".Equals(input))
            {
                var task = new Task(() => { });
                task.Start();
                task.Wait();

                output = task;
                return true;
            }

            output = null;
            return false;
        }

        private static Task<T> CreateTask<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
