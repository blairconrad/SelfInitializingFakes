namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public static class BinarySerialization
    {
        public interface IService
        {
            void VoidMethod(string s, out int i, ref DateTime dt);

            IDictionary<string, Guid> NonVoidMethod();
        }

        [Scenario]
        public static void SerializeVoidCall(
            string path,
            IRecordedCallRepository repository,
            IService realServiceWhileRecording,
            int voidMethodOutInteger,
            DateTime voidMethodRefDateTime,
            IDictionary<string, Guid> nonVoidMethodResult)
        {
            "Given a file path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            "And a BinaryFileRecordedCallRepository targeting that path"
                .x(() => repository = new BinaryFileRecordedCallRepository(path));

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<IService>();

                    int i;
                    DateTime dt = DateTime.MinValue;
                    A.CallTo(() => realServiceWhileRecording.VoidMethod("firstCallKey", out i, ref dt))
                        .AssignsOutAndRefParameters(17, new DateTime(2017, 1, 24));

                    A.CallTo(() => realServiceWhileRecording.NonVoidMethod())
                        .Returns(new Dictionary<string, Guid>
                        {
                            ["key1"] = new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"),
                        });
                });

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<IService>.For(() => realServiceWhileRecording, repository))
                    {
                        var fake = fakeService.Object;
                        fake.VoidMethod("firstCallKey", out voidMethodOutInteger, ref voidMethodRefDateTime);
                        nonVoidMethodResult = fake.NonVoidMethod();
                    }
                });

            "And I use a self-initializing fake in playback mode"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<IService>.For<IService>(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        int i;
                        DateTime dt = DateTime.MinValue;
                        fake.VoidMethod("blah", out i, ref dt);
                    }
                });

            "Then the recording fake forwards calls to the wrapped service"
                .x(() =>
                {
                    int i;
                    DateTime dt = DateTime.MinValue;
                    A.CallTo(() => realServiceWhileRecording.VoidMethod(A<string>._, out i, ref dt))
                        .MustHaveHappened();
                    A.CallTo(() => realServiceWhileRecording.NonVoidMethod()).MustHaveHappened();
                });

            "And the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    voidMethodOutInteger.Should().Be(17);
                    voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
                    nonVoidMethodResult.Should()
                        .HaveCount(1).And
                        .ContainKey("key1")
                        .WhichValue.Should().Be(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                });
        }

        private static IService UnusedFactory() => null!;
    }
}
