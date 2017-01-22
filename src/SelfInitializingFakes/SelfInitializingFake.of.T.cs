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
    public class SelfInitializingFake<TService> : IDisposable
    {
        private static readonly ConcurrentDictionary<IFakeObjectCall, RecordedCall> RecordedCalls =
            new ConcurrentDictionary<IFakeObjectCall, RecordedCall>();

        private readonly IRecordedCallRepository repository;
        private readonly IList<RecordedCall> recordedCalls;
        private readonly Queue<RecordedCall> expectedCalls;
        private Exception recordingException;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfInitializingFake{TService}"/> class.
        /// </summary>
        /// <param name="serviceFactory">A factory that will create a concrete factory if needed.</param>
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
                this.Fake = A.Fake<TService>(options => options.Wrapping(wrappedService));
                this.recordedCalls = new List<RecordedCall>();
                this.AddRecordingRulesToFake(wrappedService);
            }
            else
            {
                this.Fake = A.Fake<TService>();
                this.expectedCalls = new Queue<RecordedCall>(callsFromRepository);
                this.AddPlaybackRulesToFake();
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

        private static RecordedCall GetRecordedCall(IFakeObjectCall call)
        {
            RecordedCall recordedCall;
            RecordedCalls.TryRemove(call, out recordedCall);
            return recordedCall;
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
            A.CallTo(this.Fake).AssignsOutAndRefParametersLazily(call =>
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
            // occur after ReturnsLazily.
            A.CallTo(this.Fake).WithNonVoidReturnType()

                // FakeItEasy 2.3.2 and below will intercept void methods even when WhenNonVoidReturnType is
                // specified, so constrain the call by return type again.
                .Where(call => call.Method.ReturnType != typeof(void))
                .ReturnsLazily(call =>
                {
                    try
                    {
                        var recordedCall = BuildRecordedCall(call, target);
                        RecordedCalls[call] = recordedCall;
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
                .AssignsOutAndRefParametersLazily(call => GetRecordedCall(call).OutAndRefValues);
        }

        private void AddDisposedRecordingRuleToFake()
        {
            A.CallTo(this.Fake)
                .Throws(new RecordingException("The fake has been disposed and can record no more calls."));
        }

        private void AddPlaybackRulesToFake()
        {
            A.CallTo(this.Fake)
                .AssignsOutAndRefParametersLazily(call => this.ConsumeNextExpectedCall(call).OutAndRefValues);

            // These rule relies on an undocumented FakeItEasy behavior: that
            // the actions specified in AssignsOutAndRefParametersLazily will
            // occur after ReturnsLazily.
            A.CallTo(this.Fake).WithNonVoidReturnType()
                .ReturnsLazily(call =>
                {
                    RecordedCall recordedCall = this.ConsumeNextExpectedCall(call);
                    RecordedCalls[call] = recordedCall;
                    return recordedCall.ReturnValue;
                })
                .AssignsOutAndRefParametersLazily(call => GetRecordedCall(call).OutAndRefValues);
        }
    }
}
