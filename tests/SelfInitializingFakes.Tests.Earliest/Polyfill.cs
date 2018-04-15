namespace FakeItEasy
{
    using FakeItEasy.Configuration;

    public static class Polyfill 
    {
        public static UnorderedCallAssertion MustHaveHappenedTwiceExactly(this IAssertConfiguration configuration)
        {
            return configuration.MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}