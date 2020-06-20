namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;

    public static class BinarySerialization
    {
        [Scenario]
        public static void SerializeVoidCall(
            string path,
            IRecordedCallRepository repository,
            int voidMethodOutInteger,
            DateTime voidMethodRefDateTime,
            IDictionary<string, Guid> dictionaryMethodResult)
        {
            "Given a file path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            "And a BinaryFileRecordedCallRepository targeting that path"
                .x(() => repository = new BinaryFileRecordedCallRepository(path));

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ISampleService>.For(() => new SampleService(), repository))
                    {
                        DateTime discardDateTime = DateTime.MaxValue;
                        var fake = fakeService.Object;
                        fake.VoidMethod("firstCallKey", out _, ref discardDateTime);
                        _ = fake.DictionaryReturningMethod();
                    }
                });

            "And I use a self-initializing fake in playback mode"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ISampleService>.For(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        fake.VoidMethod("firstCallKey", out voidMethodOutInteger, ref voidMethodRefDateTime);
                        dictionaryMethodResult = fake.DictionaryReturningMethod();
                    }
                });

            "Then the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    voidMethodOutInteger.Should().Be(17);
                    voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
                    dictionaryMethodResult.Should()
                        .HaveCount(1).And
                        .ContainKey("key1")
                        .WhichValue.Should().Be(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                });
        }

        private static ISampleService UnusedFactory() => null!;
    }
}
