namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Saves and loads recorded calls made to a service in a file.
    /// </summary>
    public abstract class FileBasedRecordedCallRepository : IRecordedCallRepository
    {
        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBasedRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        protected FileBasedRecordedCallRepository(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Saves recorded calls for later use.
        /// </summary>
        /// <param name="calls">The recorded calls to save.</param>
        public void Save(IEnumerable<RecordedCall> calls)
        {
            using (var fileStream = File.Open(this.path, FileMode.Create))
            {
                this.WriteToStream(calls, fileStream);
            }
        }

        /// <summary>
        /// Loads and returns saved calls.
        /// </summary>
        /// <returns>The saved calls, or <c>null</c> if there are none.</returns>
        public IEnumerable<RecordedCall> Load()
        {
            if (!File.Exists(this.path))
            {
                return null;
            }

            using (var fileStream = File.OpenRead(this.path))
            {
                return this.ReadFromStream(fileStream);
            }
        }

        /// <summary>
        /// Writes calls to a file.
        /// </summary>
        /// <param name="calls">The calls.</param>
        /// <param name="fileStream">The stream that writes to the file.</param>
        protected abstract void WriteToStream(IEnumerable<RecordedCall> calls, FileStream fileStream);

        /// <summary>
        /// Reads calls from a file.
        /// </summary>
        /// <param name="fileStream">The stream that reads from the file.</param>
        /// <returns>The deserialized calls.</returns>
        protected abstract IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream);
    }
}
