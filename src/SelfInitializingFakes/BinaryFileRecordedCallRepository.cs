namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Saves and loads recorded calls made to a service. The calls will be saved to a file,
    /// serialized using the .NET Framework's built-in object serializer.
    /// </summary>
    public class BinaryFileRecordedCallRepository : IRecordedCallRepository
    {
        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        public BinaryFileRecordedCallRepository(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Saves recorded calls for later use.
        /// </summary>
        /// <param name="calls">The recorded calls to save.</param>
        public void Save(IEnumerable<RecordedCall> calls)
        {
            var formatter = new BinaryFormatter();
            using (var file = File.Open(this.path, FileMode.Create))
            {
                formatter.Serialize(file, calls.ToArray());
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

            var formatter = new BinaryFormatter();
            using (var file = File.OpenRead(this.path))
            {
                return (IEnumerable<RecordedCall>)formatter.Deserialize(file);
            }
        }
    }
}
