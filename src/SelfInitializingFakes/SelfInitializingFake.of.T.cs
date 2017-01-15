namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FakeItEasy;
    using FakeItEasy.Core;
    using SelfInitializingFakes.Infrastructure;

    /// <summary>
    /// A self-initializing fake that will delegate to an actual service when first
    /// used, and create a script that will be fulfilled by a fake object on subsequent uses.
    /// </summary>
    /// <typeparam name="TService">The type of the service to fake.</typeparam>
    public class SelfInitializingFake<TService>
    {
        private static readonly ConcurrentDictionary<IFakeObjectCall, ICallData> CallDatas =
            new ConcurrentDictionary<IFakeObjectCall, ICallData>();

        private readonly ICallDataRepository repository;
        private readonly IList<ICallData> recordedCalls;
        private readonly Queue<ICallData> expectedCalls;
        private Exception recordingException;

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
                this.recordedCalls = new List<ICallData>();
                this.AddRecordingRulesToFake(wrappedService);
            }
            else
            {
                this.Fake = A.Fake<TService>();
                this.expectedCalls = new Queue<ICallData>(callsFromRepository);

                A.CallTo(this.Fake)
                    .Invokes(call =>
                    {
                        ICallData callData = this.ConsumeNextExpectedCall(call);
                        CallDatas[call] = callData;
                    })
                    .AssignsOutAndRefParametersLazily(call =>
                    {
                        ICallData callData;
                        CallDatas.TryRemove(call, out callData);
                        return callData.OutAndRefValues;
                    });

                A.CallTo(this.Fake).WithNonVoidReturnType()
                    .ReturnsLazily(call =>
                    {
                        ICallData callData = this.ConsumeNextExpectedCall(call);
                        CallDatas[call] = callData;
                        return callData.ReturnValue;
                    })
                    .AssignsOutAndRefParametersLazily(call =>
                    {
                        ICallData callData;
                        CallDatas.TryRemove(call, out callData);
                        return callData.OutAndRefValues;
                    });
            }
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in the tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in the tests.</value>
        public TService Fake { get; }

        private bool IsRecording => this.recordedCalls != null;

        /// <summary>
        /// Ends a recording or playback session. In recording mode, ensures that captured
        /// calls are persisted for subsequent sessions. In playback mode, causes captured
        /// calls to be verified.
        /// </summary>
        public void EndSession()
        {
            if (this.IsRecording)
            {
                if (this.recordingException != null)
                {
                    throw new PlaybackException(
                        "error encountered while recording actual service calls",
                        this.recordingException);
                }

                this.repository.Save(this.recordedCalls);
            }
        }

        private ICallData ConsumeNextExpectedCall(IFakeObjectCall call)
        {
            if (this.expectedCalls.Count == 0)
            {
                throw new PlaybackException($"expected no more calls, but found [{call.Method}]");
            }

            var expectedCall = this.expectedCalls.Dequeue();
            if (expectedCall.Method != call.Method)
            {
                throw new PlaybackException($"expected a call to [{expectedCall.Method}], but found [{call.Method}]");
            }

            return expectedCall;
        }

        private void AddRecordingRulesToFake<TClass>(TClass actual)
        {
            A.CallTo(this.Fake).Invokes(call =>
            {
                call.Method.Invoke(actual, call.Arguments.ToArray());
                this.recordedCalls.Add(new CallData { Method = call.Method });
            });

            A.CallTo(this.Fake).WithNonVoidReturnType()
                .ReturnsLazily(call =>
            {
                try
                {
                    var arguments = call.Arguments.ToArray();
                    var result = call.Method.Invoke(actual, arguments);

                    var outAndRefValues = new List<object>();
                    int index = 0;
                    foreach (var parameter in call.Method.GetParameters())
                    {
                        if (parameter.ParameterType.IsByRef)
                        {
                            outAndRefValues.Add(arguments[index]);
                        }

                        ++index;
                    }

                    var callData = new CallData
                    {
                        Method = call.Method,
                        ReturnValue = result,
                        OutAndRefValues = outAndRefValues.ToArray(),
                    };

                    CallDatas[call] = callData;
                    this.recordedCalls.Add(callData);
                    return result;
                }
                catch (Exception e)
                {
                    var serviceException = e.InnerException ?? e;
                    if (this.recordingException == null)
                    {
                        this.recordingException = serviceException;
                    }

                    serviceException.Rethrow();
                    return null; // to satisfy the compiler
                }
            })
            .AssignsOutAndRefParametersLazily(call =>
                {
                    ICallData callData;
                    CallDatas.TryRemove(call, out callData);
                    return callData.OutAndRefValues;
                });
        }

        private class CallData : ICallData
        {
            public MethodInfo Method { get; set; }

            public object ReturnValue { get; set; }

            public object[] OutAndRefValues { get; set; }
        }
    }
}
