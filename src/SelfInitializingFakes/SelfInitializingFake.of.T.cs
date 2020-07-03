namespace SelfInitializingFakes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FakeItEasy;
    using SelfInitializingFakes.Infrastructure;

    /// <summary>
    /// A self-initializing fake that will delegate to an actual service when first
    /// used, and create a script that will be fulfilled by a fake object on subsequent uses.
    /// </summary>
    /// <typeparam name="TService">The type of the service to fake.</typeparam>
    public sealed class SelfInitializingFake<TService> : IDisposable
        where TService : class
    {
        private static readonly ITypeConverter TypeConverter = new CompoundTypeConverter(new TaskTypeConverter(), new LazyTypeConverter());

        private readonly IRecordedCallRepository repository;
        private readonly RecordingRule? recordingRule;

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
                this.recordingRule = new RecordingRule(wrappedService, TypeConverter);
                Fake.GetFakeManager(this.Object).AddRuleFirst(this.recordingRule);
            }
            else
            {
                this.Object = A.Fake<TService>();
                Fake.GetFakeManager(this.Object).AddRuleFirst(new PlaybackRule(
                    new Queue<RecordedCall>(callsFromRepository),
                    TypeConverter));
            }
        }

        /// <summary>
        /// Gets the fake <typeparamref name="TService"/> to be used in tests.
        /// </summary>
        /// <value>The fake <typeparamref name="TService"/> to be used in tests.</value>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "Object", Justification = "The term Object does not refer to the type System.Object.")]
        public TService Object { get; }

        /// <summary>
        /// Creates a new self-initializing fake <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TConcreteService">The type of the service the factory will produce.</typeparam>
        /// <param name="serviceFactory">A factory that will create a concrete service if needed.</param>
        /// <param name="repository">A source of saved call information, or sink for the same.</param>
        /// <returns>A new self-initializing fake <typeparamref name="TService"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case where the type parameter acts as an entry point into the fluent api.")]
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
            if (this.recordingRule != null)
            {
                this.recordingRule.ThrowIfFailed();
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
