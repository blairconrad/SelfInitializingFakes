namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FakeItEasy;

    /// <summary>
    /// A self-initializing fake that will delegate to an actual service when first
    /// used, and create a script that will be fulfilled by a fake object on subsequent uses.
    /// </summary>
    /// <typeparam name="TService">The type of the service to fake.</typeparam>
    public class SelfInitializingFake<TService>
    {
        private readonly ICallDataRepository repository;
        private readonly IList<ICallData> savedCalls;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfInitializingFake{TService}"/> class.
        /// </summary>
        /// <param name="serviceFactory">A factory that will create a concrete factory if needed.</param>
        /// <param name="repository">A source of saved call information, or sink for the same.</param>
        internal SelfInitializingFake(Func<TService> serviceFactory, ICallDataRepository repository)
        {
            if (serviceFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.repository = repository;

            var callsFromRepository = this.repository.Load();
            if (callsFromRepository == null)
            {
                var wrappedService = serviceFactory.Invoke();
                this.Fake = A.Fake<TService>(options => options.Wrapping(wrappedService));
                this.savedCalls = new List<ICallData>();
                this.AddRecordingRulesToFake(wrappedService);
            }
            else
            {
                this.Fake = A.Fake<TService>();
                foreach (var savedCall in callsFromRepository.Reverse())
                {
                    A.CallTo(this.Fake).WithNonVoidReturnType()
                        .Where(call => Equals(call.Method, savedCall.Method))
                        .Returns(savedCall.ReturnValue).Once();
                }
            }
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in the tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in the tests.</value>
        public TService Fake { get; }

        private bool IsRecording => this.savedCalls != null;

        /// <summary>
        /// Ends a recording or playback session. In recording mode, ensures that captured
        /// calls are persisted for subsequent sessions. In playback mode, causes captured
        /// calls to be verified.
        /// </summary>
        public void EndSession()
        {
            if (this.IsRecording)
            {
                this.repository.Save(this.savedCalls);
            }
        }

        private void AddRecordingRulesToFake<TClass>(TClass actual)
        {
            A.CallTo(this.Fake).Invokes(call =>
            {
                call.Method.Invoke(actual, call.Arguments.ToArray());
                this.savedCalls.Add(new CallData { Method = call.Method });
            });

            A.CallTo(this.Fake).WithNonVoidReturnType().ReturnsLazily(call =>
            {
                var result = call.Method.Invoke(actual, call.Arguments.ToArray());
                this.savedCalls.Add(new CallData { Method = call.Method, ReturnValue = result });
                return result;
            });
        }

        private class CallData : ICallData
        {
            public MethodInfo Method { get; set; }

            public object ReturnValue { get; set; }
        }
    }
}
