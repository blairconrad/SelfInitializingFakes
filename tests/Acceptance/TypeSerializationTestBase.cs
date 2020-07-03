namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using FluentAssertions;
    using SelfInitializingFakes.Tests.Acceptance.Helpers;
    using Xbehave;
    using Xunit;

    public abstract class TypeSerializationTestBase
    {
        /// <summary>Create common test cases for <see cref="SerializeCommonCalls(string, IRecordedCallRepository)" />.</summary>
        public static IEnumerable<object[]> TestCases()
        {
            return typeof(TypeSerializationTestBase).GetNestedTypes(BindingFlags.NonPublic)
#if FRAMEWORK_TYPE_LACKS_ISABSTRACT
               .Where(t => !t.GetTypeInfo().IsAbstract)
#else
               .Where(t => !t.IsAbstract)
#endif
               .Where(t => typeof(TestCase).IsAssignableFrom(t))
               .Select(t => new object[] { Activator.CreateInstance(t) });
        }

        [MemberData(nameof(TestCases))]
        [Scenario]
        public void SerializeCommonCalls(TestCase testCase, IRecordedCallRepository repository)
        {
            "Given a recorded call repository"
                .x(() => repository = this.CreateRepository());

            "When I use a self-initializing fake in recording mode"
                .x(() =>
                {
                    using (var fakeService = SelfInitializingFake<ISampleService>.For(() => new SampleService(), repository))
                    {
                        var fake = fakeService.Object;
                        testCase.Record(fake);
                    }
                });

            "And I use a self-initializing fake in playback mode"
                .x(() =>
                {
                    using (var playbackFakeService = SelfInitializingFake<ISampleService>.For(UnusedFactory, repository))
                    {
                        var fake = playbackFakeService.Object;
                        testCase.Playback(fake);
                    }
                });

            "Then the playback fake returns the recorded out and ref parameters and results"
                .x(() =>
                {
                    testCase.Verify();
                });
        }

        protected static ISampleService UnusedFactory() => null!;

        protected abstract IRecordedCallRepository CreateRepository();

        public abstract class TestCase
        {
            public abstract void Record(ISampleService service);

            public abstract void Playback(ISampleService service);

            public abstract void Verify();
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodWithOutIntegerAndRefDateTime : TestCase
        {
            private int voidMethodOutInteger;
            private DateTime voidMethodRefDateTime;

            public override void Record(ISampleService service)
            {
                DateTime discardDateTime = DateTime.MinValue;
                service.VoidMethod("firstCallKey", out _, ref discardDateTime);
            }

            public override void Playback(ISampleService service)
            {
                service.VoidMethod("firstCallKey", out this.voidMethodOutInteger, ref this.voidMethodRefDateTime);
            }

            public override void Verify()
            {
                this.voidMethodOutInteger.Should().Be(17);
                this.voidMethodRefDateTime.Should().Be(new DateTime(2017, 1, 24));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsLazyInt : TestCase
        {
            private Lazy<int>? result;

            public override void Record(ISampleService service)
            {
                service.LazyIntReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.LazyIntReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsValueCreated.Should().BeFalse();
                this.result!.Value.Should().Be(3);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsLazyString : TestCase
        {
            private Lazy<string>? result;

            public override void Record(ISampleService service)
            {
                service.LazyStringReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.LazyStringReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsValueCreated.Should().BeFalse();
                this.result!.Value.Should().Be("three");
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodWithOutLazyInt : TestCase
        {
            private Lazy<int>? lazyInt;

            public override void Record(ISampleService service)
            {
                service.MethodWithLazyOut(out _);
            }

            public override void Playback(ISampleService service)
            {
                service.MethodWithLazyOut(out this.lazyInt);
            }

            public override void Verify()
            {
                this.lazyInt!.IsValueCreated.Should().BeFalse();
                this.lazyInt!.Value.Should().Be(-14);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsTask : TestCase
        {
            private Task? result;

            public override void Record(ISampleService service)
            {
                service.TaskReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.TaskReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsCompleted.Should().BeTrue();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsTaskInt : TestCase
        {
            private Task<int>? result;

            public override void Record(ISampleService service)
            {
                service.TaskIntReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.TaskIntReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsCompleted.Should().BeTrue();
                this.result!.Result.Should().Be(5);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsLazyTaskInt : TestCase
        {
            private Lazy<Task<int>>? result;

            public override void Record(ISampleService service)
            {
                service.LazyTaskIntReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.LazyTaskIntReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsValueCreated.Should().BeFalse();
                this.result!.Value.IsCompleted.Should().BeTrue();
                this.result!.Value.Result.Should().Be(19);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Required for testing.")]
        private class MethodThatReturnsTaskLazyInt : TestCase
        {
            private Task<Lazy<int>>? result;

            public override void Record(ISampleService service)
            {
                service.TaskLazyIntReturningMethod();
            }

            public override void Playback(ISampleService service)
            {
                this.result = service.TaskLazyIntReturningMethod();
            }

            public override void Verify()
            {
                this.result!.IsCompleted.Should().BeTrue();
                this.result!.Result.IsValueCreated.Should().BeFalse();
                this.result!.Result.Value.Should().Be(18);
            }
        }
    }
}