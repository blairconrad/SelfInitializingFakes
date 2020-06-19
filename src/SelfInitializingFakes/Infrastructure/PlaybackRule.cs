namespace SelfInitializingFakes.Infrastructure
{
    using System.Collections.Generic;
    using FakeItEasy.Core;

    /// <summary>
    /// A rule that defines the behaviour of a fake during playback.
    /// </summary>
    internal class PlaybackRule : IFakeObjectCallRule
    {
        private readonly Queue<RecordedCall> expectedCalls;
        private readonly ITypeConverter typeConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackRule"/> class.
        /// </summary>
        /// <param name="expectedCalls">The calls that are expected to be made on the fake.</param>
        /// <param name="typeConverter">A helper to convert values from serialized variants to their original representations.</param>
        public PlaybackRule(Queue<RecordedCall> expectedCalls, ITypeConverter typeConverter)
        {
            this.expectedCalls = expectedCalls;
            this.typeConverter = typeConverter;
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
            this.SetReturnValue(fakeObjectCall, recordedCall);
            this.SetOutAndRefValues(fakeObjectCall, recordedCall);
        }

        /// <summary>
        /// Determines whether this rule applies to the intercepted call.
        /// This rule applies to all calls.
        /// </summary>
        /// <param name="fakeObjectCall">The call to check.</param>
        /// <returns><c>true</c> all the time.</returns>
        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall) => true;

        private void SetReturnValue(IInterceptedFakeObjectCall fakeObjectCall, RecordedCall recordedCall)
        {
            var returnValue = recordedCall.ReturnValue;
            if (this.typeConverter.ConvertForPlayback(fakeObjectCall.Method.ReturnType, returnValue, out object? convertedReturnValue))
            {
                returnValue = convertedReturnValue;
            }

            fakeObjectCall.SetReturnValue(returnValue);
        }

        private void SetOutAndRefValues(IInterceptedFakeObjectCall fakeObjectCall, RecordedCall recordedCall)
        {
            int outOrRefIndex = 0;
            for (int parameterIndex = 0; parameterIndex < fakeObjectCall.Method.GetParameters().Length; parameterIndex++)
            {
                var parameter = fakeObjectCall.Method.GetParameters()[parameterIndex];
                if (parameter.ParameterType.IsByRef)
                {
                    var parameterValue = recordedCall.OutAndRefValues[outOrRefIndex++];
                    if (this.typeConverter.ConvertForPlayback(
                            parameter.ParameterType.GetElementType(),
                            parameterValue,
                            out object? convertedParameterValue))
                    {
                        parameterValue = convertedParameterValue;
                    }

                    fakeObjectCall.SetArgumentValue(parameterIndex, parameterValue);
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
