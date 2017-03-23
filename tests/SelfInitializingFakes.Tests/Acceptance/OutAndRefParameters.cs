namespace SelfInitializingFakes.Tests.Acceptance
{
    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;

    public static class OutAndRefParameters
    {
        public interface IService
        {
            bool TryToSetSomeOutAndRefParameters(out int @out, ref int @ref);

            void SetSomeOutAndRefParameters(out int @out, ref int @ref);
        }

        [Scenario]
        public static void NonVoidOutAndRef(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            IService realServiceWhileRecording,
            IService realServiceDuringPlayback,
            int recordingOut,
            int recordingRef,
            bool recordingReturn,
            int playbackOut,
            int playbackRef,
            bool playbackReturn)
        {
            "Given a call storage object"
                .x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<IService>();

                    int localOut;
                    int localRef = 0;
                    A.CallTo(() => realServiceWhileRecording.TryToSetSomeOutAndRefParameters(out localOut, ref localRef))
                        .WithAnyArguments()
                        .Returns(true)
                        .AssignsOutAndRefParameters(19, 8);
                });

            "When I use a self-initializing fake in recording mode to try to set some out and ref parameters"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;
                        recordingReturn = fake.TryToSetSomeOutAndRefParameters(out recordingOut, ref recordingRef);
                    }
                });

            "And I use a self-initializing fake in playback mode to try to set some out and ref parameters"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake.For<IService>(() => null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;
                        playbackReturn = fake.TryToSetSomeOutAndRefParameters(out playbackOut, ref playbackRef);
                    }
                });

            "Then the recording fake sets the out parameter to the value set by the wrapped service"
                .x(() => recordingOut.Should().Be(19));

            "And it sets the ref parameter to the value set by the wrapped service"
                .x(() => recordingRef.Should().Be(8));

            "And it returns the value that the wrapped service did"
                .x(() => recordingReturn.Should().BeTrue());

            "Then the playback fake sets the out parameter to the value seen in recording mode"
                .x(() => playbackOut.Should().Be(19));

            "And it sets the ref parameter to the value seen in recording mode"
                .x(() => playbackRef.Should().Be(8));

            "And it returns the value seen in recording mode"
                .x(() => playbackReturn.Should().BeTrue());
        }

        [Scenario]
        public static void VoidOutAndRef(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            IService realServiceWhileRecording,
            IService realServiceDuringPlayback,
            int recordingOut,
            int recordingRef,
            bool recordingReturn,
            int playbackOut,
            int playbackRef,
            bool playbackReturn)
        {
            "Given a call storage object"
                .x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<IService>();

                    int localOut;
                    int localRef = 0;
                    A.CallTo(() => realServiceWhileRecording.SetSomeOutAndRefParameters(out localOut, ref localRef))
                        .WithAnyArguments()
                        .AssignsOutAndRefParameters(7, -14);
                });

            "When I use a self-initializing fake in recording mode to set some out and ref parameters"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;
                        fake.SetSomeOutAndRefParameters(out recordingOut, ref recordingRef);
                    }
                });

            "And I use a self-initializing fake in playback mode to set some out and ref parameters"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake.For<IService>(() => null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;
                        fake.SetSomeOutAndRefParameters(out playbackOut, ref playbackRef);
                    }
                });

            "Then the recording fake sets the out parameter to the value set by the wrapped service"
                .x(() => recordingOut.Should().Be(7));

            "And it sets the ref parameter to the value set by the wrapped service"
                .x(() => recordingRef.Should().Be(-14));

            "Then the playback fake sets the out parameter to the value seen in recording mode"
                .x(() => playbackOut.Should().Be(7));

            "And it sets the ref parameter to the value seen in recording mode"
                .x(() => playbackRef.Should().Be(-14));
        }
    }
}
