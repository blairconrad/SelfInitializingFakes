namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FakeItEasy;

    /// <summary>
    /// Creates self-initializing fakes; fakes that will delegate to an actual service when first
    /// used, creating a script that will be used on subsequent uses.
    /// </summary>
    /// <typeparam name="TService">The type of the service to fake.</typeparam>
    public class SelfInitializingFake<TService>
    {
        private readonly ICallDataRepository repository;
        private readonly IEnumerable<ICallData> savedCalls;

        private SelfInitializingFake(TService fake, ICallDataRepository repository, IEnumerable<ICallData> savedCalls)
        {
            this.Fake = fake;
            this.repository = repository;
            this.savedCalls = savedCalls;
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in the tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in the tests.</value>
        public TService Fake { get; }

        /// <summary>
        /// Creates a new self-initializing fake <typeparamref name="TService"/>.
        /// </summary>
        /// <param name="serviceFactory">A factory that will create a concrete factory if needed.</param>
        /// <param name="repository">A source of saved call information, or sink for the same.</param>
        /// <returns>A new self-initializing fake <typeparamref name="TService"/>.</returns>
        public static SelfInitializingFake<TService> For(Func<TService> serviceFactory, ICallDataRepository repository)
        {
            if (serviceFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            TService fake;
            var savedCalls = repository.Load();
            if (savedCalls == null)
            {
                var wrappedService = serviceFactory.Invoke();
                fake = A.Fake<TService>(options => options.Wrapping(wrappedService));
                savedCalls = SetupRecordingFake(wrappedService, fake);
            }
            else
            {
                fake = A.Fake<TService>();
                foreach (var savedCall in savedCalls.Reverse())
                {
                    A.CallTo(fake).WithNonVoidReturnType()
                        .Where(call => Equals(call.Method, savedCall.Method))
                        .Returns(savedCall.ReturnValue).Once();
                }
            }

            return new SelfInitializingFake<TService>(fake, repository, savedCalls);
        }

        /// <summary>
        /// Ends a recording or playback session. In recording mode, ensures that captured
        /// calls are persised for subsequent sessions. In playback mode, causes captured
        /// calls to be verified.
        /// </summary>
        public void EndSession()
        {
            this.repository.Save(this.savedCalls);
        }

        private static List<CallData> SetupRecordingFake<TClass>(TClass actual, TClass fake)
        {
            var newSavedCalls = new List<CallData>();

            A.CallTo(fake).Invokes(call =>
            {
                call.Method.Invoke(actual, call.Arguments.ToArray());
                newSavedCalls.Add(new CallData { Method = call.Method });
            });

            A.CallTo(fake).WithNonVoidReturnType().ReturnsLazily(call =>
            {
                var result = call.Method.Invoke(actual, call.Arguments.ToArray());
                newSavedCalls.Add(new CallData { Method = call.Method, ReturnValue = result });
                return result;
            });

            return newSavedCalls;
        }

        private class CallData : ICallData
        {
            public MethodInfo Method { get; set; }

            public object ReturnValue { get; set; }
        }
    }
}
