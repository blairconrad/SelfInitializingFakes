namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.IO;
    using FluentAssertions;
    using Xbehave;

    public static class XmlSerialization
    {
        public interface IService
        {
            void VoidMethod(string s, out int i, ref DateTime dt);

            Guid NonVoidMethod();
        }

        [Scenario]
        public static void SerializeVoidCall(
            string path,
            IRecordedCallRepository repository,
            int voidMethodOutInteger,
            DateTime voidMethodRefDateTime,
            Guid nonVoidMethodResult)
        {
            "Given a file path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"));

            "And a XmlFileRecordedCallRepository targeting that path"
                .x(() => repository = new XmlFileRecordedCallRepository(path));

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<IService>.For(() => new Service(), repository))
                    {
                        DateTime discardDateTime = DateTime.MaxValue;
                        var fake = fakeService.Object;
                        fake.VoidMethod("recordingCallKey", out _, ref discardDateTime);
                        _ = fake.NonVoidMethod();
                    }
                });

            "And I use a self-initializing fake in playback mode"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<IService>.For<IService>(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        fake.VoidMethod("blah", out voidMethodOutInteger, ref voidMethodRefDateTime);
                        nonVoidMethodResult = fake.NonVoidMethod();
                    }
                });

            "Then the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    voidMethodOutInteger.Should().Be(17);
                    voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
                    nonVoidMethodResult.Should().Be(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                });
        }

        private static IService UnusedFactory() => null!;

        private class Service : IService
        {
            public void VoidMethod(string s, out int i, ref DateTime dt)
            {
                i = 17;
                dt = new DateTime(2017, 1, 24);
            }

            public Guid NonVoidMethod() => new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd");
        }
    }
}
