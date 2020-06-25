namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using FakeItEasy;
    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helper;
    using Xbehave;
    using Xunit;

    public static class FileBasedSerializers
    {
        public interface IService
        {
            Guid NonVoidMethod();
        }

        public static IEnumerable<object[]> ConcreteRepositoryTypes() =>
            typeof(FileBasedRecordedCallRepository).GetConcreteSubTypesInAssembly()
                .Select(t => new object[] { t });

        [Scenario]
        [MemberData(nameof(ConcreteRepositoryTypes))]
        public static void SerializeToDirectoryThatDoesNotExist(
            Type concreteRepositoryType,
            string closestExistingDirectory,
            string missingChildDirectory,
            string repositoryPath,
            IRecordedCallRepository repository,
            IService realServiceWhileRecording)
        {
            "Given a directory that exists"
                .x(() =>
                {
                    closestExistingDirectory = Path.GetTempPath();
                    Directory.Exists(closestExistingDirectory).Should().BeTrue("the temp directory should exist");
                });

            "And a subdirectory that doesn't exist"
                .x(() =>
                {
                    missingChildDirectory = Path.Combine(closestExistingDirectory, Guid.NewGuid().ToString());
                    Directory.Exists(missingChildDirectory).Should().BeFalse("the subdirectory should not exist");
                });

            "And a path pointing at a file inside a directory inside the subdirectory"
                .x(() => repositoryPath = Path.Combine(missingChildDirectory, "subsub", "repofile"));

            $"And a {concreteRepositoryType} targeting that path"
                .x(() => repository = (IRecordedCallRepository)Activator.CreateInstance(concreteRepositoryType, repositoryPath));

            "And a real service to wrap while recording"
                .x(() => realServiceWhileRecording = A.Fake<IService>());

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using var fakeService = SelfInitializingFake<IService>.For(() => realServiceWhileRecording, repository);
                    var fake = fakeService.Object;
                    _ = fake.NonVoidMethod();
                });

            "Then the repository file is created"
                .x(() => File.Exists(repositoryPath).Should().BeTrue("the file should exist"));
        }
    }
}
