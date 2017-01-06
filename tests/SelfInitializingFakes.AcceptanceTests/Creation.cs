namespace SelfInitializingFakes.AcceptanceTests
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class Creation
    {
        public interface IService
        {
        }

        [Scenario]
        public static void CreatingFromNullServiceFactory(
            ISavedCallRepository repository,
            Func<IService> serviceFactory,
            Exception exception)
        {
            "Given a saved call repository"
                .x(() => repository = A.Fake<ISavedCallRepository>());

            "Given a null service factory"
                .x(() => serviceFactory = null);

            "When I create a self-initializing fake from the factory"
                .x(() => exception = Record.Exception(() => new SelfInitializingFake<IService>(repository, serviceFactory)));

            "Then the constructor throws an exception"
                .x(() => exception.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("serviceFactory"));
        }

        [Scenario]
        public static void CreatingFromNullCallRepository(
            ISavedCallRepository repository,
            Func<IService> serviceFactory,
            Exception exception)
        {
            "Given a null saved call repository"
                .x(() => repository = null);

            "And a service factory"
                .x(() => serviceFactory = A.Fake<Func<IService>>());

            "When I create a self-initializing fake from the factory"
                .x(() => exception = Record.Exception(() => new SelfInitializingFake<IService>(repository, serviceFactory)));

            "Then the constructor throws an exception"
                .x(() => exception.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("repository"));
        }
    }
}
