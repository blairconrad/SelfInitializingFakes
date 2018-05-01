namespace SelfInitializingFakes.Infrastructure
{
    using System.Collections.Generic;
    using FakeItEasy.Core;

    internal class PlaybackRule : IFakeObjectCallRule
    {
        private Queue<RecordedCall> expectedCalls;

        public PlaybackRule(Queue<RecordedCall> expectedCalls)
        {
            this.expectedCalls = expectedCalls;
        }

        public int? NumberOfTimesToCall => null;

        public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
        {
            RecordedCall recordedCall = this.ConsumeNextExpectedCall(fakeObjectCall);
            SetReturnValue(fakeObjectCall, recordedCall);
            SetOutAndRefValues(fakeObjectCall, recordedCall);
        }

        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall) => true;

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

        private static void SetReturnValue(IInterceptedFakeObjectCall fakeObjectCall, RecordedCall recordedCall)
        {
            fakeObjectCall.SetReturnValue(recordedCall.ReturnValue);
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
    }
}
