namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;
    using Xunit;

    public static class ServiceThrowsExceptions
    {
        public interface IService
        {
            string Function();

            void Action();
        }

        [Scenario]
        public static void NonVoidMethodThrowsExceptionWhileRecording(
            InMemoryStorage inMemoryStorage,
            IService realServiceWhileRecording,
            Exception originalException,
            Exception exceptionWhileRecording,
            Exception exceptionWhileEndingRecordingSession)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<IService>());

            "And the real service throws an exception when a non-void method is called"
                .x(() =>
                {
                    A.CallTo(() => realServiceWhileRecording.Function())
                        .Throws(originalException = new InvalidOperationException());
                });

            "When I use a self-initializing fake in recording mode to execute the method"
                .x(() =>
                {
                    var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);
                    var fake = fakeService.Fake;

                    exceptionWhileRecording = Record.Exception(() => fake.Function());

                    exceptionWhileEndingRecordingSession = Record.Exception(() => fakeService.EndSession());
                });

            "Then the recording fake throws the original exception"
                .x(() => exceptionWhileRecording.Should().BeSameAs(originalException));

            "But ending the recording session throws a playback exception"
                .x(() => exceptionWhileEndingRecordingSession.Should().BeOfType<PlaybackException>()
                    .Which.Message.Should().Be("error encountered while recording actual service calls"));

            "And the session-ending exception has the original exception as its inner exception"
                .x(() => exceptionWhileEndingRecordingSession.InnerException.Should().BeSameAs(originalException));
        }

        [Scenario]
        public static void VoidMethodThrowsExceptionWhileRecording(
            InMemoryStorage inMemoryStorage,
            IService realServiceWhileRecording,
            Exception originalException,
            Exception exceptionWhileRecording,
            Exception exceptionWhileEndingRecordingSession)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<IService>());

            "And the real service throws an exception when executing a void method"
                .x(() =>
                {
                    A.CallTo(() => realServiceWhileRecording.Action())
                        .Throws(originalException = new InvalidOperationException());
                });

            "When I use a self-initializing fake in recording mode to execute the method"
                .x(() =>
                {
                    var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);
                    var fake = fakeService.Fake;

                    exceptionWhileRecording = Record.Exception(() => fake.Action());

                    exceptionWhileEndingRecordingSession = Record.Exception(() => fakeService.EndSession());
                });

            "Then the recording fake throws the original exception"
                .x(() => exceptionWhileRecording.Should().BeSameAs(originalException));

            "But ending the recording session throws a playback exception"
                .x(() => exceptionWhileEndingRecordingSession.Should().BeOfType<PlaybackException>()
                    .Which.Message.Should().Be("error encountered while recording actual service calls"));

            "And the session-ending exception has the original exception as its inner exception"
                .x(() => exceptionWhileEndingRecordingSession.InnerException.Should().BeSameAs(originalException));
        }
    }
}
