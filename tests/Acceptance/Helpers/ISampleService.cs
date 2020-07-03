namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISampleService
    {
        void VoidMethod(string s, out int i, ref DateTime dt);

        Guid GuidReturningMethod();

        IDictionary<string, Guid> DictionaryReturningMethod();

        Lazy<int> LazyIntReturningMethod();

        Lazy<string> LazyStringReturningMethod();

        void MethodWithLazyOut(out Lazy<int> lazyInt);

        Task TaskReturningMethod();

        Task<int> TaskIntReturningMethod();

        Lazy<Task<int>> LazyTaskIntReturningMethod();

        Task<Lazy<int>> TaskLazyIntReturningMethod();
    }
}