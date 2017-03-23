namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy;
    using FakeItEasy.Core;
    using SelfInitializingFakes.Infrastructure;

    /// <summary>
    /// A self-initializing fake that will delegate to an actual service when first
    /// used, and create a script that will be fulfilled by a fake object on subsequent uses.
    /// </summary>
    /// <typeparam name="TService">The type of the service to fake.</typeparam>
    public class SelfInitializingFake<TService> : IDisposable
    {
        // FakeItEasy provides one path for setting the return value of a fake call, and one for
        // setting the out and ref values to be applied, with no way to link them up and configure
        // the call in one step. In recording mode, we'll be figuring out the return value and out
        // and ref values from a single call to the wrapped service, so we'll use this cache to
        // (very) temporarily store the out and ref values so they can be applied to the call.
        // The same thing is done during playback.
        private static readonly ConcurrentDictionary<IFakeObjectCall, object[]> OutAndRefValuesCache =
            new ConcurrentDictionary<IFakeObjectCall, object[]>();

        private readonly IRecordedCallRepository repository;
        private readonly IList<RecordedCall> recordedCalls;
        private readonly Queue<RecordedCall> expectedCalls;
        private Exception recordingException;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfInitializingFake{TService}"/> class.
        /// </summary>
        /// <param name="serviceFactory">A factory that will create a concrete service if needed.</param>
        /// <param name="repository">A source of saved call information, or sink for the same.</param>
        internal SelfInitializingFake(Func<TService> serviceFactory, IRecordedCallRepository repository)
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
                this.Object = A.Fake<TService>(options => options.Wrapping(wrappedService));
                this.recordedCalls = new List<RecordedCall>();
                this.AddRecordingRulesToFake(wrappedService);
            }
            else
            {
                this.Object = A.Fake<TService>();
                this.expectedCalls = new Queue<RecordedCall>(callsFromRepository);
                this.AddPlaybackRulesToFake();
            }
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in tests.</value>
        public TService Object { get; }

        private bool IsRecording => this.recordedCalls != null;

        /// <summary>
        /// Ends a recording or playback session.
        /// In recording mode, ensures that captured calls are persisted for subsequent sessions.
        /// In playback mode, causes captured calls to be verified.
        /// </summary>
        public void Dispose()
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

            this.AddDisposedRecordingRuleToFake();
        }

        private static object[] GetOutAndRefValues(IFakeObjectCall call)
        {
            object[] outAndRefValues;
            OutAndRefValuesCache.TryRemove(call, out outAndRefValues);
            return outAndRefValues;
        }

        private static RecordedCall BuildRecordedCall<TClass>(IFakeObjectCall call, TClass target)
        {
            var arguments = call.Arguments.ToArray();
            var result = call.Method.Invoke(target, arguments);

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

            return new RecordedCall
            {
                Method = call.Method.ToString(),
                ReturnValue = result,
                OutAndRefValues = outAndRefValues.ToArray(),
            };
        }

        private RecordedCall ConsumeNextExpectedCall(IFakeObjectCall call)
        {
            if (this.expectedCalls.Count == 0)
            {
                throw new PlaybackException($"expected no more calls, but found [{call.Method}]");
            }

            var expectedCall = this.expectedCalls.Dequeue();
            if (expectedCall.Method != call.Method.ToString())
            {
                throw new PlaybackException($"expected a call to [{expectedCall.Method}], but found [{call.Method}]");
            }

            return expectedCall;
        }

        private void AddRecordingRulesToFake<TClass>(TClass target)
        {
            // This rule applies to all calls to the fake, but is
            // overridden for calls with return values belowe.
            A.CallTo(this.Object).AssignsOutAndRefParametersLazily(call =>
            {
                try
                {
                    var recordedCall = BuildRecordedCall(call, target);
                    this.recordedCalls.Add(recordedCall);
                    return recordedCall.OutAndRefValues;
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
            });

            // This rule relies on an undocumented FakeItEasy behavior: that
            // the actions specified in AssignsOutAndRefParametersLazily will
            // be invoked after ReturnsLazily.
            A.CallTo(this.Object).WithNonVoidReturnType()
                .ReturnsLazily(call =>
                {
                    try
                    {
                        var recordedCall = BuildRecordedCall(call, target);
                        OutAndRefValuesCache[call] = recordedCall.OutAndRefValues;
                        this.recordedCalls.Add(recordedCall);
                        return recordedCall.ReturnValue;
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
                .AssignsOutAndRefParametersLazily(GetOutAndRefValues);
        }

        private void AddDisposedRecordingRuleToFake()
        {
            A.CallTo(this.Object)
                .Throws(new RecordingException("The fake has been disposed and can record no more calls."));
        }

        private void AddPlaybackRulesToFake()
        {
            A.CallTo(this.Object)
                .AssignsOutAndRefParametersLazily(call => this.ConsumeNextExpectedCall(call).OutAndRefValues);

            // This rule relies on an undocumented FakeItEasy behavior: that
            // the actions specified in AssignsOutAndRefParametersLazily will
            // be invoked after ReturnsLazily.
            A.CallTo(this.Object).WithNonVoidReturnType()
                .ReturnsLazily(call =>
                {
                    RecordedCall recordedCall = this.ConsumeNextExpectedCall(call);
                    OutAndRefValuesCache[call] = recordedCall.OutAndRefValues;
                    return recordedCall.ReturnValue;
                })
                .AssignsOutAndRefParametersLazily(GetOutAndRefValues);
        }
    }
}
