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
    public class XmlFileRecordedCallRepository : IRecordedCallRepository
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(RecordedCall[]));

        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        public XmlFileRecordedCallRepository(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Saves recorded calls for later use.
        /// </summary>
        /// <param name="calls">The recorded calls to save.</param>
        public void Save(IEnumerable<RecordedCall> calls)
        {
            using (var fileStream = File.Create(this.path))
            {
                Serializer.Serialize(fileStream, calls.ToArray());
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
                return (IEnumerable<RecordedCall>)Serializer.Deserialize(fileStream);
            }
        }
    }
}
