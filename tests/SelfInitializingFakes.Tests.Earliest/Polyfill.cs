namespace FakeItEasy
{
    using FakeItEasy.Configuration;

    internal static class Polyfill
    {
        public static UnorderedCallAssertion MustHaveHappenedTwiceExactly(this IAssertConfiguration configuration)
        {
            return configuration.MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
