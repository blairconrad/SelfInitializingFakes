namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.IO;

    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;

    public static class XmlSerialization
    {
        [Scenario]
        public static void SerializeVoidCall(
            string path,
            IRecordedCallRepository repository,
            int voidMethodOutInteger,
            DateTime voidMethodRefDateTime,
            Guid guidMethodResult)
        {
            "Given a file path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"));

            "And a XmlFileRecordedCallRepository targeting that path"
                .x(() => repository = new XmlFileRecordedCallRepository(path));

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ISampleService>.For(() => new SampleService(), repository))
                    {
                        DateTime discardDateTime = DateTime.MaxValue;
                        var fake = fakeService.Object;
                        fake.VoidMethod("recordingCallKey", out _, ref discardDateTime);
                        _ = fake.GuidReturningMethod();
                    }
                });

            "And I use a self-initializing fake in playback mode"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ISampleService>.For<ISampleService>(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        fake.VoidMethod("blah", out voidMethodOutInteger, ref voidMethodRefDateTime);
                        guidMethodResult = fake.GuidReturningMethod();
                    }
                });

            "Then the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    voidMethodOutInteger.Should().Be(17);
                    voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
                    guidMethodResult.Should().Be(new Guid("5b61d48f-e9e5-49ad-9c51-a9aae056aa84"));
                });
        }

        private static ISampleService UnusedFactory() => null!;
    }
}
