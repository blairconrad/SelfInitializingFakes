namespace SelfInitializingFakes.AcceptanceTests
{
    using FluentAssertions;
    using Xbehave;

    public static class Class1
    {
        [Scenario]
        public static void W()
        {
            "Given"
                .x(() => "hello".Should().Be("hello"));
        }
    }
}
