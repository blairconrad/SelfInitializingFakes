namespace SelfInitializingFakes.Tests.Conventions
{
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;

    public class ExportedTypesNamespaces
    {
        [Fact]
        public void Exported_types_should_have_SelfInitializingFakes_namespace()
        {
            typeof(SelfInitializingFake).GetTypeInfo().Assembly.GetExportedTypes()
                .Where(t => t.Namespace != "SelfInitializingFakes")
                .Should().BeEmpty();
        }

        [Fact]
        public void All_types_in_SelfInitializingFakes_namespace_should_be_exported()
        {
            typeof(SelfInitializingFake).GetTypeInfo().Assembly.GetTypes()
                .Where(t => t.Namespace == "SelfInitializingFakes" && !t.GetTypeInfo().IsVisible)
                .Should().BeEmpty();
        }
    }
}
