namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System;
    using System.Collections.Generic;

    public interface ISampleService
    {
        void VoidMethod(string s, out int i, ref DateTime dt);

        Guid GuidReturningMethod();

        IDictionary<string, Guid> DictionaryReturningMethod();

        Lazy<int> LazyIntReturningMethod();

        Lazy<string> LazyStringReturningMethod();

        void MethodWithLazyOut(out Lazy<int> lazyInt);
    }
}