namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy.Core;

    /// <summary>
    /// A rule that defines the behaviour of a fake during the recording phase.
    /// Generally speaking, forwards calls to a target and retains the results, which
    /// can be retrieved from <see cref="RecordedCalls"/>.
    /// </summary>
    internal class RecordingRule : IFakeObjectCallRule
    {
        private readonly object target;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingRule"/> class.
        /// </summary>
        /// <param name="target">The object to which to forward calls, in order to harvest return, out, and ref values.</param>
        public RecordingRule(object target)
        {
            this.target = target;
        }

        /// <summary>
        /// Gets the number of calls for which the rule is valid. This rule has no expiration.
        /// </summary>
        public int? NumberOfTimesToCall => null;

        /// <summary>
        /// Gets or sets the first exception encountered while recording calls.
        /// </summary>
        public Exception RecordingException { get; set; }

        /// <summary>
        /// Gets the calls encountered while recording.
        /// </summary>
        public IList<RecordedCall> RecordedCalls { get; } = new List<RecordedCall>();

        /// <summary>
        /// Determines whether this rule applies to the intercepted call.
        /// This rule applies to all calls.
        /// </summary>
        /// <param name="fakeObjectCall">The call to check.</param>
        /// <returns><c>true</c> all the time.</returns>
        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall) => true;

        /// <summary>
        /// Forwards the received call onto the wrapped target, records the results, and
        /// applies any out, ref, or return values, so the caller sees them.
        /// </summary>
        /// <param name="fakeObjectCall">The intercepted call.</param>
        public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
        {
            try
            {
                var recordedCall = this.BuildRecordedCall(fakeObjectCall);
                this.RecordedCalls.Add(recordedCall);
                ApplyRecordedCall(recordedCall, fakeObjectCall);
            }
            catch (Exception e)
            {
                var serviceException = e.InnerException ?? e;
                if (this.RecordingException == null)
                {
                    this.RecordingException = serviceException;
                }

                serviceException.Rethrow();
            }
        }

        private static void ApplyRecordedCall(RecordedCall recordedCall, IInterceptedFakeObjectCall fakeObjectCall)
        {
            fakeObjectCall.SetReturnValue(recordedCall.ReturnValue);

            int outAndRefIndex = 0;
            int parameterIndex = 0;
            foreach (var parameter in fakeObjectCall.Method.GetParameters())
            {
                if (parameter.ParameterType.IsByRef)
                {
                    fakeObjectCall.SetArgumentValue(parameterIndex, recordedCall.OutAndRefValues[outAndRefIndex++]);
                }

                ++parameterIndex;
            }
        }

        private RecordedCall BuildRecordedCall(IFakeObjectCall call)
        {
            var arguments = call.Arguments.ToArray();
            var result = call.Method.Invoke(this.target, arguments);

            var outAndRefValues = new List<object>();
            int index = 0;
            foreach (var parameter in call.Method.GetParameters())
            {
                if (parameter.ParameterType.IsByRef)
                {
                    outAndRefValues.Add(arguments[index]);
                }

                ++index;
            }

            return new RecordedCall
            {
                Method = call.Method.ToString(),
                ReturnValue = result,
                OutAndRefValues = outAndRefValues.ToArray(),
            };
        }
    }
}
