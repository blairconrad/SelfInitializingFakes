namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Saves and loads recorded calls made to a service. The calls will be saved to a file,
    /// serialized as XML.
    /// </summary>
    public class XmlFileRecordedCallRepository : FileBasedRecordedCallRepository
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(RecordedCall[]));

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        public XmlFileRecordedCallRepository(string path)
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
            Serializer.Serialize(fileStream, calls.ToArray());
        }

        /// <summary>
        /// Reads calls from a file stream.
        /// </summary>
        /// <param name="fileStream">The stream</param>
        /// <returns>The deserialized calls.</returns>
        protected override IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream)
        {
            return (IEnumerable<RecordedCall>)Serializer.Deserialize(fileStream);
        }
    }
}
