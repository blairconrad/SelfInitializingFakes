namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Saves and loads recorded calls made to a service. The calls will be saved to a fileStream,
    /// serialized using the .NET Framework's built-in object serializer.
    /// </summary>
    public class BinaryFileRecordedCallRepository : FileBasedRecordedCallRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The fileStream to save calls to, or load them from.</param>
        public BinaryFileRecordedCallRepository(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Writes calls to a file stream.
        /// </summary>
        /// <param name="calls">The calls.</param>
        /// <param name="fileStream">The stream</param>
        protected override void WriteToStream(IEnumerable<RecordedCall> calls, FileStream fileStream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, calls.ToArray());
        }

        /// <summary>
        /// Reads calls from a file stream.
        /// </summary>
        /// <param name="fileStream">The stream</param>
        /// <returns>The deserialized calls.</returns>
        protected override IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream)
        {
            var formatter = new BinaryFormatter();
            return (IEnumerable<RecordedCall>)formatter.Deserialize(fileStream);
        }
    }
}
