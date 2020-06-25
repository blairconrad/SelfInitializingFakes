namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Saves and loads recorded calls made to a service. The calls will be saved to a file,
    /// via a supplied <see cref="FileStream"/>, serialized using the .NET Framework's built-in object serializer.
    /// </summary>
    public class BinaryFileRecordedCallRepository : FileBasedRecordedCallRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="pathComponents">
        /// The file to save calls to, or load them from.
        /// May be a complete filename, or path components that will be combined.
        /// If not present, the containing directory will be created on save.
        /// </param>
        public BinaryFileRecordedCallRepository(params string[] pathComponents)
            : base(pathComponents)
        {
        }

        /// <summary>
        /// Writes calls to a file.
        /// </summary>
        /// <param name="calls">The calls.</param>
        /// <param name="fileStream">The stream that writes to the file.</param>
        protected override void WriteToStream(IEnumerable<RecordedCall> calls, FileStream fileStream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, calls.ToArray());
        }

        /// <summary>
        /// Reads calls from a file.
        /// </summary>
        /// <param name="fileStream">The stream that reads from the file.</param>
        /// <returns>The deserialized calls.</returns>
        protected override IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream)
        {
            var formatter = new BinaryFormatter();
            return (IEnumerable<RecordedCall>)formatter.Deserialize(fileStream);
        }
    }
}
