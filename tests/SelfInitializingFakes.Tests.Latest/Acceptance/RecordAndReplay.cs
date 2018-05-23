namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;
    using Xunit;

    public static class RecordAndReplay
    {
        public interface ILibraryService
        {
            int GetCount(string internationalStandardBookNumber);

            string GetTitle(string internationalStandardBookNumber);
        }

        [Scenario]
        public static void ReplaysRecordedCalls(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            ILibraryService realServiceWhileRecording,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback)
        {
            "Given a call storage object".x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<ILibraryService>();

                    A.CallTo(() => realServiceWhileRecording.GetCount("1"))
                        .ReturnsNextFromSequence(0x1A, 0x1B);
                    A.CallTo(() => realServiceWhileRecording.GetCount("2"))
                        .Returns(0x2);
                });

            "When I use a self-initializing fake in recording mode to get the counts for book 1, 2, and 1 again"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ILibraryService>.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;
                        countsWhileRecording = new List<int>
                        {
                            fake.GetCount("1"),
                            fake.GetCount("2"),
                            fake.GetCount("1"),
                        };
                    }
                });

            "And I use a self-initializing fake in playback mode to get the counts for book 1, 2, and 1 again"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ILibraryService>.For(() => (ILibraryService)null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;
                        countsDuringPlayback = new List<int>
                        {
                            fake.GetCount("1"),
                            fake.GetCount("2"),
                            fake.GetCount("1"),
                        };
                    }
                });

            "Then the recording fake forwards calls to the wrapped service"
                .x(() => A.CallTo(() => realServiceWhileRecording.GetCount("1"))
                    .MustHaveHappenedTwiceExactly());

            "And the recording fake returns the wrapped service's results"
                .x(() => countsWhileRecording.Should().Equal(0x1A, 0x2, 0x1B));

            "And the playback fake returns the recorded results"
                .x(() => countsDuringPlayback.Should().Equal(0x1A, 0x2, 0x1B));
        }

        [Scenario]
        public static void IgnoresParameterValuesInRecordedCalls(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            ILibraryService realServiceWhileRecording,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback)
        {
            "Given a call storage object"
                .x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<ILibraryService>();

                    A.CallTo(() => realServiceWhileRecording.GetCount("1"))
                        .Returns(0x1);
                    A.CallTo(() => realServiceWhileRecording.GetCount("2"))
                        .Returns(0x2);
                });

            "When I use a self-initializing fake in recording mode to get the counts for book 2 and 1"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ILibraryService>.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;

                        countsWhileRecording = new List<int>
                        {
                            fake.GetCount("2"),
                            fake.GetCount("1"),
                        };
                    }
                });

            "And I use a self-initializing fake in playback mode to get the counts for book 1 and 2"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ILibraryService>.For(() => (ILibraryService)null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;

                        countsDuringPlayback = new List<int>
                        {
                            fake.GetCount("1"),
                            fake.GetCount("2"),
                        };
                    }
                });

            "Then the recording fake returns the wrapped service's results"
                .x(() => countsWhileRecording.Should().Equal(0x2, 0x1));

            // These results demonstrate that the self-initializing fake relies on a script
            // defined by which methods are called, without regard to the arguments
            // passed to the methods.
            "And the playback fake returns results in 'recorded order'"
                .x(() => countsDuringPlayback.Should().Equal(0x2, 0x1));
        }

        [Scenario]
        public static void ThrowsIfWrongCallEncountered(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            ILibraryService realServiceWhileRecording,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<ILibraryService>());

            "When I use a self-initializing fake in recording mode to get the count and title for book 1"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ILibraryService>.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;

                        fake.GetCount("1");
                        fake.GetTitle("1");
                    }
                });

            "And I use a self-initializing fake in playback mode to get the title for book 1"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ILibraryService>.For(() => (ILibraryService)null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;

                        exception = Record.Exception(() => fake.GetTitle("1"));
                    }
                });

            // This result demonstrates that the self-initializing fake relies on a script
            // defined by which methods are called, and is completely inflexible with
            // regard to the order of calls.
            "Then the playback fake throws a playback exception"
                .x(() => exception.Should()
                    .BeOfType<PlaybackException>().Which.Message.Should()
                    .Be(
                        "expected a call to [Int32 GetCount(System.String)], but found [System.String GetTitle(System.String)]"));
        }

        [Scenario]
        public static void ThrowsIfTooManyCallsEncountered(
            InMemoryRecordedCallRepository inMemoryRecordedCallRepository,
            ILibraryService realServiceWhileRecording,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryRecordedCallRepository = new InMemoryRecordedCallRepository());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<ILibraryService>());

            "When I use a self-initializing fake in recording mode to get the count and title for book 1"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ILibraryService>.For(() => realServiceWhileRecording, inMemoryRecordedCallRepository))
                    {
                        var fake = fakeService.Object;

                        fake.GetCount("1");
                        fake.GetTitle("1");
                    }
                });

            "And I use a self-initializing fake in playback mode to get the count and title and count for book 1"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ILibraryService>.For(() => (ILibraryService)null, inMemoryRecordedCallRepository))
                    {
                        var fake = playbackFakeService.Object;

                        fake.GetCount("1");
                        fake.GetTitle("1");
                        exception = Record.Exception(() => fake.GetCount("1"));
                    }
                });

            // This result demonstrates that the self-initializing fake relies on a script
            // defined by which methods are called, and is completely inflexible with
            // regard to the number of repetitions of the calls.
            "Then the playback fake throws a playback exception"
                .x(() => exception.Should()
                    .BeOfType<PlaybackException>().Which.Message.Should()
                    .Be("expected no more calls, but found [Int32 GetCount(System.String)]"));
        }
    }
}
