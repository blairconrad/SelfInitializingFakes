namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy.Core;

    internal class RecordingRule : IFakeObjectCallRule
    {
        private readonly object target;

        public RecordingRule(object target)
        {
            this.target = target;
        }

        public int? NumberOfTimesToCall => null;

        public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
        {
            try
            {
                var recordedCall = BuildRecordedCall(fakeObjectCall);
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

        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall) => true;

        public Exception RecordingException { get; set; }

        public IList<RecordedCall> RecordedCalls { get; } = new List<RecordedCall>();

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
