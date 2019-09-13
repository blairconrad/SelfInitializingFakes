namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
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
        /// Writes calls to a file.
        /// </summary>
        /// <param name="calls">The calls.</param>
        /// <param name="fileStream">The stream that writes to the file.</param>
        protected override void WriteToStream(IEnumerable<RecordedCall> calls, FileStream fileStream)
        {
            Serializer.Serialize(fileStream, calls.ToArray());
        }

        /// <summary>
        /// Reads calls from a file.
        /// </summary>
        /// <param name="fileStream">The stream that reads from the file.</param>
        /// <returns>The deserialized calls.</returns>
        protected override IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream)
        {
            using (var reader = XmlReader.Create(fileStream))
            {
#pragma warning disable CA3075 // Insecure DTD processing in XML - the framework is for testing, so presumably is run in a safe environment
                return (IEnumerable<RecordedCall>)Serializer.Deserialize(reader);
#pragma warning restore CA3075 // Insecure DTD processing in XML
            }
        }
    }
}
