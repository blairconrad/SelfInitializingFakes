namespace SelfInitializingFakes.Infrastructure
{
    using System.Collections.Generic;
    using FakeItEasy.Core;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A rule that defines the behaviour of a fake during playback.
    /// </summary>
    internal class PlaybackRule : IFakeObjectCallRule
    {
        private const string TaskTypeIdentifier = "Tasks.Task";
        private readonly Queue<RecordedCall> expectedCalls;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackRule"/> class.
        /// </summary>
        /// <param name="expectedCalls">The calls that are expected to be made on the fake.</param>
        public PlaybackRule(Queue<RecordedCall> expectedCalls)
        {
            this.expectedCalls = expectedCalls;
        }

        /// <summary>
        /// Gets the number of calls for which the rule is valid. This rule has no expiration.
        /// </summary>
        public int? NumberOfTimesToCall => null;

        /// <summary>
        /// Sets any out and ref values, and return value, from the next expected call,
        /// assuming the <paramref name="fakeObjectCall"/> matches the next expected call.
        /// </summary>
        /// <param name="fakeObjectCall">The call made to the fake.</param>
        public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
        {
            RecordedCall recordedCall = this.ConsumeNextExpectedCall(fakeObjectCall);
            SetReturnValue(fakeObjectCall, recordedCall);
            SetOutAndRefValues(fakeObjectCall, recordedCall);
        }

        /// <summary>
        /// Determines whether this rule applies to the intercepted call.
        /// This rule applies to all calls.
        /// </summary>
        /// <param name="fakeObjectCall">The call to check.</param>
        /// <returns><c>true</c> all the time.</returns>
        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall) => true;

        private static void SetReturnValue(IInterceptedFakeObjectCall fakeObjectCall, RecordedCall recordedCall)
        {
            var methodReturnType = fakeObjectCall.Method.ReturnType;

            if (methodReturnType.FullName.Contains(TaskTypeIdentifier))
            {
                var actualType = methodReturnType.GenericTypeArguments[0];
                var baseType = methodReturnType.BaseType;
                var methodInfo = baseType.GetMethod("FromResult");
                var genericMethod = methodInfo.MakeGenericMethod(actualType);

                if (recordedCall.ReturnValue is JObject)
                {
                  JObject jObject = (JObject)recordedCall.ReturnValue;
                  var convertedJObject = jObject.ToObject(actualType);
                  var taskResult = genericMethod.Invoke(null, new[] { convertedJObject });
                  fakeObjectCall.SetReturnValue(taskResult);
                }
                else
                {
                  var taskResult = genericMethod.Invoke(null, new[] { recordedCall.ReturnValue });
                  fakeObjectCall.SetReturnValue(taskResult);
                }
            }
            else
            {
                fakeObjectCall.SetReturnValue(recordedCall.ReturnValue);
            }
        }

        private static void SetOutAndRefValues(IInterceptedFakeObjectCall fakeObjectCall, RecordedCall recordedCall)
        {
            int outOrRefIndex = 0;
            for (int parameterIndex = 0; parameterIndex < fakeObjectCall.Method.GetParameters().Length; parameterIndex++)
            {
                var parameter = fakeObjectCall.Method.GetParameters()[parameterIndex];
                if (parameter.ParameterType.IsByRef)
                {
                    fakeObjectCall.SetArgumentValue(parameterIndex, recordedCall.OutAndRefValues[outOrRefIndex++]);
                }
            }
        }

        private RecordedCall ConsumeNextExpectedCall(IFakeObjectCall call)
        {
            if (this.expectedCalls.Count == 0)
            {
                throw new PlaybackException($"expected no more calls, but found [{call.Method}]");
            }

            var expectedCall = this.expectedCalls.Dequeue();
            if (expectedCall.Method != call.Method.ToString())
            {
                throw new PlaybackException($"expected a call to [{expectedCall.Method}], but found [{call.Method}]");
            }

            return expectedCall;
        }
    }
}
