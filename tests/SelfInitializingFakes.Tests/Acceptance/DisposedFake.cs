namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;
    using Xunit;

    public static class DisposedFake
    {
        public interface IService
        {
            void Action();

            int Function();
        }

        [Scenario]
        public static void CannotRecordVoidMethodAfterDisposing(
            InMemoryStorage inMemoryStorage,
            IService realServiceWhileRecording,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback,
            SelfInitializingFake<IService> fakeService,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<IService>());

            "And a self-initializing fake wrapping the service"
                .x(() => fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage));

            "And the fake is disposed"
                .x(() => fakeService.Dispose());

            "When I record a call to a void method using the fake"
                .x(() => exception = Record.Exception(() => fakeService.Fake.Action()));

            "Then the fake throws an exception"
                .x(() => exception.Should()
                    .BeOfType<RecordingException>()
                    .Which.Message.Should().Be("The fake has been disposed and can record no more calls."));
        }

        [Scenario]
        public static void CannotRecordNonVoidAfterDisposing(
            InMemoryStorage inMemoryStorage,
            IService realServiceWhileRecording,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback,
            SelfInitializingFake<IService> fakeService,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<IService>());

            "And a self-initializing fake wrapping the service"
                .x(() => fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage));

            "And the fake is disposed"
                .x(() => fakeService.Dispose());

            "When I record a call to a non-void method using the fake"
                .x(() => exception = Record.Exception(() => fakeService.Fake.Function()));

            "Then the fake throws an exception"
                .x(() => exception.Should()
                    .BeOfType<RecordingException>()
                    .Which.Message.Should().Be("The fake has been disposed and can record no more calls."));
        }
    }
}
