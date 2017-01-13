namespace SelfInitializingFakes.Tests.Conventions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;

    public class Naming
    {
        [Fact]
        public void All_exception_names_should_end_with_Exception()
        {
            typeof(SelfInitializingFake<>).GetTypeInfo().Assembly.GetTypes()
                .Where(t => typeof(Exception).IsAssignableFrom(t) && !t.Name.EndsWith("Exception"))
                .Should().BeEmpty();
        }

        [Fact]
        public void All_classes_called_Exception_should_be_exceptions()
        {
            typeof(SelfInitializingFake<>).GetTypeInfo().Assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Exception") && !typeof(Exception).IsAssignableFrom(t))
                .Should().BeEmpty();
        }
    }
}
