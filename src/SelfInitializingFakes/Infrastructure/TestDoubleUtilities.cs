namespace SelfInitializingFakes.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Utilities to assist with setting up and disposing of test doubles (required for recording).
    /// </summary>
    public class TestDoubleUtilities : IDisposable
    {
        private readonly List<Action> fakesToDispose = new List<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDoubleUtilities"/> class.
        /// </summary>
        /// <param name="testRecordingsBasePath">the base path for test recordings.</param>
        /// <param name="testClassName">Name of the test class.</param>
        /// <param name="testName">Name of the test.</param>
        public TestDoubleUtilities(string testRecordingsBasePath, string testClassName, string testName)
        {
            var basePath = Path.Combine(testRecordingsBasePath, testClassName, testName);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            this.BasePath = basePath;
        }

        /// <summary>
        /// Gets or sets The Base Path for the test recordings.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Creates the Self Initializing Test Double.
        /// </summary>
        /// <param name="createRealService">A method which creates the real service.</param>
        /// <typeparam name="T">The type for the service being doubled/faked.</typeparam>
        /// <returns>an instance of the test double.</returns>
        public SelfInitializingFake<T> CreateSelfInitializingTestDouble<T>(Func<T> createRealService)
            where T : class
        {
            var typeName = typeof(T).Name;

            var recordedFilePath = Path.Combine(this.BasePath, typeName + ".json");
            if (!File.Exists(recordedFilePath))
            {
                Console.WriteLine(recordedFilePath + " Does not exist??? This is only normal on first run. ");
            }

            var fakeRepo = new JsonFileRecordedCallRepository(recordedFilePath);
            var selfInitializingFake = SelfInitializingFake<T>.For(
                createRealService,
                fakeRepo);

            this.fakesToDispose.Add(selfInitializingFake.Dispose);

            return selfInitializingFake;
        }

        /// <summary>
        /// Dispose of all test doubles - which saves the recordings.
        /// </summary>
        public void Dispose() // Implement IDisposable
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of all test doubles - which saves the recordings.
        /// </summary>
        /// <param name="disposing">disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            foreach (var fakeDisposeMethod in this.fakesToDispose)
            {
                fakeDisposeMethod?.Invoke();
            }
        }
    }
}
