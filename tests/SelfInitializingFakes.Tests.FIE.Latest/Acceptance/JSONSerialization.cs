namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;

    public static class JSONSerialization
    {
        public interface IService
        {
            void VoidMethod(string s, out int i, ref DateTime dt);

            Guid NonVoidMethod();

            Task<int> AsyncMethod();
        }

        [Scenario]
        public static void SerializeVoidCall(
            string path,
            IRecordedCallRepository repository,
            IService realServiceWhileRecording,
            int voidMethodOutInteger,
            DateTime voidMethodRefDateTime,
            Guid nonVoidMethodResult,
            Task<int> asyncMethodResult)
        {
            "Given a file path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"));

            "And a XmlFileRecordedCallRepository targeting that path"
                .x(() => repository = new JsonFileRecordedCallRepository(path));

            "And a real service to wrap while recording"
                .x(() =>
                {
                    realServiceWhileRecording = A.Fake<IService>();

                    int i;
                    DateTime dt = DateTime.MinValue;
                    A.CallTo(() => realServiceWhileRecording.VoidMethod("firstCallKey", out i, ref dt))
                        .AssignsOutAndRefParameters(17, new DateTime(2017, 1, 24));

                    A.CallTo(() => realServiceWhileRecording.NonVoidMethod())
                        .Returns(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                    A.CallTo(() => realServiceWhileRecording.AsyncMethod())
                        .Returns(Task.FromResult(1234));
                });

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<IService>.For(() => realServiceWhileRecording, repository))
                    {
                        var fake = fakeService.Object;
                        fake.VoidMethod("firstCallKey", out voidMethodOutInteger, ref voidMethodRefDateTime);
                        nonVoidMethodResult = fake.NonVoidMethod();
                        asyncMethodResult = fake.AsyncMethod();
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
#if BUG_ASSIGNING_REF_VALUE_CLEARS_INCOMING_VALUE
                    DateTime dt = new DateTime(2017, 1, 24);
#else
                    DateTime dt = DateTime.MinValue;
#endif
                    A.CallTo(() => realServiceWhileRecording.VoidMethod(A<string>._, out i, ref dt))
                        .MustHaveHappened();
                    A.CallTo(() => realServiceWhileRecording.NonVoidMethod()).MustHaveHappened();
                });

            "And the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    voidMethodOutInteger.Should().Be(17);
                    voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
                    nonVoidMethodResult.Should().Be(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                    asyncMethodResult.Result.Should().Be(1234);
                });
        }

        private static IService UnusedFactory() => null!;
    }
}
