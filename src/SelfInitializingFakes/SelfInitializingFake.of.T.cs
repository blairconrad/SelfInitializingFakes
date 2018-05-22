namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
        private readonly RecordingRule recordingRule;

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

            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));

            var callsFromRepository = this.repository.Load();
            if (callsFromRepository == null)
            {
                var wrappedService = serviceFactory.Invoke();
                this.Object = A.Fake<TService>();
                this.recordingRule = new RecordingRule(wrappedService);
                Fake.GetFakeManager(this.Object).AddRuleFirst(this.recordingRule);
            }
            else
            {
                this.Object = A.Fake<TService>();
                Fake.GetFakeManager(this.Object).AddRuleFirst(new PlaybackRule(new Queue<RecordedCall>(callsFromRepository)));
            }
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in tests.</value>
        public TService Object { get; }

        private bool IsRecording => this.recordingRule != null;

        /// <summary>
        /// Creates a new self-initializing fake <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TConcreteService">The type of the service the factory will produce.</typeparam>
        /// <param name="serviceFactory">A factory that will create a concrete service if needed.</param>
        /// <param name="repository">A source of saved call information, or sink for the same.</param>
        /// <returns>A new self-initializing fake <typeparamref name="TService"/>.</returns>
        public static SelfInitializingFake<TService> For<TConcreteService>(
            Func<TConcreteService> serviceFactory,
            IRecordedCallRepository repository)
            where TConcreteService : TService
        {
            if (serviceFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return new SelfInitializingFake<TService>(() => serviceFactory.Invoke(), repository);
        }

        /// <summary>
        /// Ends a recording or playback session.
        /// In recording mode, ensures that captured calls are persisted for subsequent sessions.
        /// In playback mode, causes captured calls to be verified.
        /// </summary>
        public void Dispose()
        {
            if (this.IsRecording)
            {
                if (this.recordingRule.RecordingException != null)
                {
                    throw new RecordingException(
                        "error encountered while recording actual service calls",
                        this.recordingRule.RecordingException);
                }

                this.repository.Save(this.recordingRule.RecordedCalls);
            }

            this.AddDisposedRecordingRuleToFake();
        }

        private void AddDisposedRecordingRuleToFake()
        {
            A.CallTo(this.Object)
                .Throws(new RecordingException("The fake has been disposed and can record no more calls."));
        }
    }
}
