namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;

    public class BinarySerialization : TypeSerializationTestBase
    {
        [Scenario]
        public void SerializeCallWithDictionary(
            IRecordedCallRepository repository,
            IDictionary<string, Guid> dictionaryMethodResult)
        {
            "Given a recorded call repository"
                .x(() => repository = this.CreateRepository());

            "When I record a dictionary-returning method via a self-initializing fake"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ISampleService>.For(() => new SampleService(), repository))
                    {
                        var fake = fakeService.Object;
                        _ = fake.DictionaryReturningMethod();
                    }
                });

            "And I play back a dictionary-returning method via a self-initializing fake"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ISampleService>.For(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        dictionaryMethodResult = fake.DictionaryReturningMethod();
                    }
                });

            "Then the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    dictionaryMethodResult.Should()
                        .HaveCount(1).And
                        .ContainKey("key1")
                        .WhichValue.Should().Be(new Guid("6c7d8912-802a-43c0-82a2-cb811058a9bd"));
                });
        }

        protected override IRecordedCallRepository CreateRepository()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            return new BinaryFileRecordedCallRepository(path);
        }
    }
}