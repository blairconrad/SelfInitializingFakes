namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class ImportedFromFakeItEasy
    {
        public interface ILibraryService
        {
            int GetCount(string internationalStandardBookNumber);

            string GetTitle(string internationalStandardBookNumber);

            bool TryToSetSomeOutAndRefParameters(out int @out, ref int @ref);
        }

        [Scenario]
        public static void SelfInitializing(
            InMemoryStorage inMemoryStorage,
            ILibraryService realServiceWhileRecording,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback)
        {
            "Given a call storage object".x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<ILibraryService>();

                    A.CallTo(() => realServiceWhileRecording.GetCount("1"))
                        .ReturnsNextFromSequence(0x1A, 0x1B);
                    A.CallTo(() => realServiceWhileRecording.GetCount("2"))
                        .Returns(0x2);
                });

            "When I use a self-initializing fake in recording mode to get the counts for book 1, 2, and 1 again".x(() =>
            {
                var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);

                var fake = fakeService.Fake;
                countsWhileRecording = new List<int>
                {
                    fake.GetCount("1"),
                    fake.GetCount("2"),
                    fake.GetCount("1"),
                };

                fakeService.EndSession();
            });

            "And I use a self-initializing fake in playback mode to get the counts for book 1, 2, and 1 again"
                .x(() =>
                {
                    var playbackFakeService = SelfInitializingFake.For<ILibraryService>(() => null, inMemoryStorage);

                    var fake = playbackFakeService.Fake;
                    countsDuringPlayback = new List<int>
                    {
                        fake.GetCount("1"),
                        fake.GetCount("2"),
                        fake.GetCount("1"),
                    };

                    playbackFakeService.EndSession();
                });

            "Then the recording fake forwards calls to the wrapped service"
                .x(() => A.CallTo(() => realServiceWhileRecording.GetCount("1"))
                    .MustHaveHappened(Repeated.Exactly.Twice));

            "And the recording fake returns the wrapped service's results"
                .x(() => countsWhileRecording.Should().Equal(0x1A, 0x2, 0x1B));

            "And the playback fake returns the recorded results"
                .x(() => countsDuringPlayback.Should().Equal(0x1A, 0x2, 0x1B));
        }

////        [Scenario]
////        public static void SelfInitializingOutAndRef(
////            InMemoryStorage inMemoryStorage,
////            ILibraryService realServiceWhileRecording,
////            ILibraryService realServiceDuringPlayback,
////            int @out,
////            int @ref)
////        {
////            "Given a call storage object"
////                .x(() => inMemoryStorage = new InMemoryStorage());
////
////            "And a real service to wrap while recording"
////                .x(() =>
////                {
////                    realServiceWhileRecording = A.Fake<ILibraryService>();
////
////                    int localOut;
////                    int localRef = 0;
////                    A.CallTo(() => realServiceWhileRecording.TryToSetSomeOutAndRefParameters(out localOut, ref localRef))
////                        .WithAnyArguments()
////                        .Returns(true)
////                        .AssignsOutAndRefParameters(19, 8);
////                });
////
////            "And a real service to wrap while playing back"
////                .x(() => realServiceDuringPlayback = A.Fake<ILibraryService>());
////
////            "When I use a self-initializing fake in recording mode to try to set some out and ref parameters"
////                .x(() =>
////                {
////                    using (var recorder = new RecordingManager(inMemoryStorage))
////                    {
////                        var fakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceWhileRecording).RecordedBy(recorder));
////
////                        int localOut;
////                        int localRef = 0;
////                        fakeService.TryToSetSomeOutAndRefParameters(out localOut, ref localRef);
////                    }
////                });
////
////            "And I use a self-initializing fake in playback mode to try to set some out and ref parameters"
////                .x(() =>
////                {
////                    using (var recorder = new RecordingManager(inMemoryStorage))
////                    {
////                        var playbackFakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceDuringPlayback).RecordedBy(recorder));
////
////                        playbackFakeService.TryToSetSomeOutAndRefParameters(out @out, ref @ref);
////                    }
////                });
////
////            "Then the playback fake sets the out parameter to the value seen in recording mode"
////                .x(() => @out.Should().Be(19));
////
////            "And it sets the ref parameter to the value seen in recording mode"
////                .x(() => @ref.Should().Be(8));
////        }
////
        [Scenario]
        public static void SelfInitializingIgnoresParameterValues(
            InMemoryStorage inMemoryStorage,
            ILibraryService realServiceWhileRecording,
            ILibraryService realServiceDuringPlayback,
            IEnumerable<int> countsWhileRecording,
            IEnumerable<int> countsDuringPlayback)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

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
                    var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);
                    var fake = fakeService.Fake;

                    countsWhileRecording = new List<int>
                    {
                        fake.GetCount("2"),
                        fake.GetCount("1"),
                    };

                    fakeService.EndSession();
                });

            "And I use a self-initializing fake in playback mode to get the counts for book 1 and 2"
                .x(() =>
                {
                    var playbackFakeService = SelfInitializingFake.For<ILibraryService>(() => null, inMemoryStorage);
                    var fake = playbackFakeService.Fake;

                    countsDuringPlayback = new List<int>
                    {
                        fake.GetCount("1"),
                        fake.GetCount("2"),
                    };

                    playbackFakeService.EndSession();
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
        public static void SelfInitializingThrowsIfWrongCallEncountered(
            InMemoryStorage inMemoryStorage,
            ILibraryService realServiceWhileRecording,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<ILibraryService>());

            "When I use a self-initializing fake in recording mode to get the count and title for book 1"
                .x(() =>
                {
                    var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);
                    var fake = fakeService.Fake;

                    fake.GetCount("1");
                    fake.GetTitle("1");

                    fakeService.EndSession();
                });

            "And I use a self-initializing fake in playback mode to get the title for book 1"
                .x(() =>
                {
                    var playbackFakeService = SelfInitializingFake.For<ILibraryService>(() => null, inMemoryStorage);
                    var fake = playbackFakeService.Fake;

                    exception = Record.Exception(() => fake.GetTitle("1"));

                    playbackFakeService.EndSession();
                });

            // This result demonstrates that the self-initializing fake relies on a script
            // defined by which methods are called, and is completely inflexible with
            // regard to the order of calls.
            "Then the playback fake throws a recording exception"
                .x(() => exception.Should().BeOfType<PlaybackException>().Which.Message.Should().Be("expected a call to [Int32 GetCount(System.String)], but found [System.String GetTitle(System.String)]"));
        }

        [Scenario]
        public static void SelfInitializingThrowsIfTooManyCallsEncountered(
            InMemoryStorage inMemoryStorage,
            ILibraryService realServiceWhileRecording,
            Exception exception)
        {
            "Given a call storage object"
                .x(() => inMemoryStorage = new InMemoryStorage());

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<ILibraryService>());

            "When I use a self-initializing fake in recording mode to get the count and title for book 1"
                .x(() =>
                {
                    var fakeService = SelfInitializingFake.For(() => realServiceWhileRecording, inMemoryStorage);
                    var fake = fakeService.Fake;

                    fake.GetCount("1");
                    fake.GetTitle("1");

                    fakeService.EndSession();
                });

            "And I use a self-initializing fake in playback mode to get the count and title and count for book 1"
                .x(() =>
                {
                    var playbackFakeService = SelfInitializingFake.For<ILibraryService>(() => null, inMemoryStorage);
                    var fake = playbackFakeService.Fake;

                    fake.GetCount("1");
                    fake.GetTitle("1");
                    exception = Record.Exception(() => fake.GetCount("1"));

                    playbackFakeService.EndSession();
                });

            // This result demonstrates that the self-initializing fake relies on a script
            // defined by which methods are called, and is completely inflexible with
            // regard to the number of repetitions of the calls.
            "Then the playback fake throws a recording exception"
                .x(
                    () =>
                        exception.Should()
                            .BeOfType<PlaybackException>()
                            .Which.Message.Should()
                            .Be("expected no more calls, but found [Int32 GetCount(System.String)]"));
        }

////        [Scenario]
////        public static void SelfInitializingExceptionWhileRecording(
////            InMemoryStorage inMemoryStorage,
////            ILibraryService realServiceWhileRecording,
////            ILibraryService realServiceDuringPlayback,
////            Exception originalException,
////            Exception exceptionWhileRecording,
////            Exception exceptionWhilePlayingBack)
////        {
////            "Given a call storage object"
////                .x(() => inMemoryStorage = new InMemoryStorage());
////
////            "And a real service to wrap while recording"
////                .x(() => realServiceWhileRecording = A.Fake<ILibraryService>());
////
////            "And the real service throws an exception when getting the title for book 1"
////                .x(() =>
////                {
////                    A.CallTo(() => realServiceWhileRecording.GetTitle("1"))
////                        .Throws(originalException = new InvalidOperationException());
////                });
////
////            "And a real service to wrap while playing back"
////                .x(() => realServiceDuringPlayback = A.Fake<ILibraryService>());
////
////            "When I use a self-initializing fake in recording mode to get the title for book 1"
////                .x(() =>
////                {
////                    using (var recorder = new RecordingManager(inMemoryStorage))
////                    {
////                        var fakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceWhileRecording).RecordedBy(recorder));
////
////                        exceptionWhileRecording = Record.Exception(() => fakeService.GetTitle("1"));
////                    }
////                });
////
////            "And I use a self-initializing fake in playback mode to get the title for book 1"
////                .x(() =>
////                {
////                    using (var recorder = new RecordingManager(inMemoryStorage))
////                    {
////                        var fakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceDuringPlayback).RecordedBy(recorder));
////
////                        exceptionWhilePlayingBack = Record.Exception(() => fakeService.GetTitle("1"));
////                    }
////                });
////
////            "Then the recording fake throws the original exception"
////                .x(() => exceptionWhileRecording.Should().BeSameAs(originalException));
////
////            "But the playback fake throws a recording exception"
////                .x(() => exceptionWhilePlayingBack.Should().BeAnExceptionOfType<RecordingException>());
////        }
////
////        [Trait("explicit", "yes")]
////        [Scenario]
////        public static void SelfInitializingWithFileRecorder(
////            string fileRecorderPath,
////            ILibraryService realServiceWhileRecording,
////            ILibraryService realServiceDuringPlayback,
////            int countWhileRecording,
////            int countDuringPlayback)
////        {
////            "Given a path that does not exist"
////                .x(() => fileRecorderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
////
////            "And a real service to wrap while recording"
////                .x(() =>
////                {
////                    realServiceWhileRecording = A.Fake<ILibraryService>();
////
////                    A.CallTo(() => realServiceWhileRecording.GetCount("8"))
////                        .Returns(0x8);
////                });
////
////            "And a real service to wrap while playing back"
////                .x(() => realServiceDuringPlayback = A.Fake<ILibraryService>());
////
////            "When I use a self-initializing fake recording to the path to get the count for book 8"
////                .x(() =>
////                {
////                    using (var recorder = Recorders.FileRecorder(fileRecorderPath))
////                    {
////                        var fakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceWhileRecording).RecordedBy(recorder));
////                        countWhileRecording = fakeService.GetCount("8");
////                    }
////                })
////                .Teardown(() => File.Delete(fileRecorderPath));
////
////            "And I use a self-initializing fake playing back from the path to get the count for book 8"
////                .x(() =>
////                {
////                    using (var recorder = Recorders.FileRecorder(fileRecorderPath))
////                    {
////                        var playbackFakeService = A.Fake<ILibraryService>(options => options
////                            .Wrapping(realServiceDuringPlayback).RecordedBy(recorder));
////
////                        countDuringPlayback = playbackFakeService.GetCount("8");
////                    }
////                });
////
////            "Then the recording fake returns the wrapped service's result"
////                .x(() => countWhileRecording.Should().Be(8));
////
////            "And the playback fake returns the recorded result"
////                .x(() => countDuringPlayback.Should().Be(8));
////        }

        public class InMemoryStorage : ICallDataRepository
        {
            private IEnumerable<ICallData> recordedCalls;

            public IEnumerable<ICallData> Load()
            {
                return this.recordedCalls;
            }

            public void Save(IEnumerable<ICallData> calls)
            {
                this.recordedCalls = calls.ToList();
            }
        }
    }
}
