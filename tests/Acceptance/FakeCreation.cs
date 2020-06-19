namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class FakeCreation
    {
        [SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "For testing. The interface doesn't need to do anything.")]
        public interface IService
        {
        }

        [Scenario]
        public static void CreateFromNullServiceFactory(
            IRecordedCallRepository repository,
            Func<IService>? serviceFactory,
            Exception exception)
        {
            "Given a null service factory"
                .x(() => serviceFactory = null);

            "And a saved call repository"
                .x(() => repository = A.Fake<IRecordedCallRepository>());

            "When I create a self-initializing fake"
                .x(() => exception = Record.Exception(() =>
                        SelfInitializingFake<IService>.For(serviceFactory!, repository)));

            "Then the constructor throws an exception"
                .x(() => exception.Should()
                    .BeOfType<ArgumentNullException>()
                    .Which.ParamName.Should().Be("serviceFactory"));
        }

        [Scenario]
        public static void CreateFromNullCallRepository(
            IRecordedCallRepository? repository,
            Func<IService> serviceFactory,
            Exception exception)
        {
            "Given a service factory"
                .x(() => serviceFactory = A.Fake<Func<IService>>());

            "And a null saved call repository"
                .x(() => repository = null);

            "When I create a self-initializing fake"
                .x(() => exception = Record.Exception(() =>
                        SelfInitializingFake<IService>.For(serviceFactory, repository!)));

            "Then the constructor throws an exception"
                .x(() => exception.Should()
                    .BeOfType<ArgumentNullException>()
                    .Which.ParamName.Should().Be("repository"));
        }

        [Scenario]
        public static void CreateFromRepositoryAndServiceFactory(
            IRecordedCallRepository repository,
            Func<IService> serviceFactory,
            SelfInitializingFake<IService> fake)
        {
            "Given a saved call repository"
                .x(() => repository = A.Fake<IRecordedCallRepository>());

            "And a service factory"
                .x(() => serviceFactory = A.Fake<Func<IService>>());

            "When I create a self-initializing fake"
                .x(() => fake = SelfInitializingFake<IService>.For(serviceFactory, repository));

            "Then the self-initializing fake is created"
                .x(() => fake.Should().NotBeNull());

            "And its Fake property is not null"
                .x(() => fake.Object.Should().NotBeNull());
        }

        [Scenario]
        public static void CreateFromInitializedRepository(
            IRecordedCallRepository repository,
            Func<IService> serviceFactory,
            SelfInitializingFake<IService> fake)
        {
            "Given a saved call repository"
                .x(() => repository = A.Fake<IRecordedCallRepository>());

            "And the repository has been initialized"
                .x(() => A.CallTo(() => repository.Load()).Returns(Enumerable.Empty<RecordedCall>()));

            "And a service factory"
                .x(() => serviceFactory = A.Fake<Func<IService>>());

            "When I create a self-initializing fake"
                .x(() => fake = SelfInitializingFake<IService>.For(serviceFactory, repository));

            "Then the factory is not invoked"
                .x(() => A.CallTo(serviceFactory).MustNotHaveHappened());
        }

        [Scenario]
        public static void CreateFromUninitializedRepository(
            IRecordedCallRepository repository,
            Func<IService> serviceFactory,
            SelfInitializingFake<IService> fake)
        {
            "Given a saved call repository"
                .x(() => repository = A.Fake<IRecordedCallRepository>());

            "And the repository has not been initialized"
                .x(() => A.CallTo(() => repository.Load()).Returns(null));

            "And a service factory"
                .x(() => serviceFactory = A.Fake<Func<IService>>());

            "When I create a self-initializing fake"
                .x(() => fake = SelfInitializingFake<IService>.For(serviceFactory, repository));

            "Then the factory is invoked to create the service"
                .x(() => A.CallTo(serviceFactory).MustHaveHappened());
        }

        [Scenario]
        public static void CreateFromDerivedFactoryType(
            IRecordedCallRepository repository,
            Func<Service> serviceFactory,
            SelfInitializingFake<IService> fake)
        {
            "Given a saved call repository"
                .x(() => repository = A.Fake<IRecordedCallRepository>());

            "And the repository has not been initialized"
                .x(() => A.CallTo(() => repository.Load()).Returns(null));

            "And a service factory that creates a derived type"
                .x(() => serviceFactory = A.Fake<Func<Service>>());

            "When I create a self-initializing fake"
                .x(() => fake = SelfInitializingFake<IService>.For<IService>(serviceFactory, repository));

            "Then the factory is invoked to create the service"
                .x(() => A.CallTo(serviceFactory).MustHaveHappened());
        }

        public class Service : IService
        {
        }
    }
}
