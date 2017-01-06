namespace SelfInitializingFakes.AcceptanceTests
{
    using System;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class Creation
    {
        public interface IService
        {
        }

        [Scenario]
        public static void CreatingFromNullServiceFactory(Func<IService> serviceFactory, Exception exception)
        {
            "Given a null service factory"
                .x(() => serviceFactory = null);

            "When I create a self-initializing fake from the factory"
                .x(() => exception = Record.Exception(() => new SelfInitializingFake<IService>(serviceFactory)));

            "Then the constructor throws an exception"
                .x(() => exception.Should().BeOfType<ArgumentNullException>());
        }
    }
}
