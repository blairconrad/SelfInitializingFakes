namespace SelfInitializingFakes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Saves and loads recorded calls made to a service. The calls will be saved to a file,
    /// serialized as XML.
    /// </summary>
    public class JsonFileRecordedCallRepository : FileBasedRecordedCallRepository
    {
        private static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
            { NullValueHandling = NullValueHandling.Include, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        private readonly JsonSerializerSettings jsonSettings;

        private readonly Newtonsoft.Json.Formatting formatting;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfInitializingFakes.JsonFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        /// <param name="jsonSettings">JSON settings.</param>
        /// /// <param name="formatting">JSON formatting.</param>
        public JsonFileRecordedCallRepository(string path, JsonSerializerSettings jsonSettings, Newtonsoft.Json.Formatting formatting)
            : base(path)
        {
            this.jsonSettings = jsonSettings;
            this.formatting = formatting;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfInitializingFakes.JsonFileRecordedCallRepository"/> class.
        /// </summary>
        /// <param name="path">The file to save calls to, or load them from.</param>
        public JsonFileRecordedCallRepository(string path)
            : this(path, DefaultJsonSettings, Newtonsoft.Json.Formatting.Indented)
        {
        }

        /// <summary>
        /// Writes calls to a file.
        /// </summary>
        /// <param name="calls">The calls.</param>
        /// <param name="fileStream">The stream that writes to the file.</param>
        protected override void WriteToStream(IEnumerable<RecordedCall> calls, FileStream fileStream)
        {
            using (var writer = new StreamWriter(fileStream))
            {
                var contents = JsonConvert.SerializeObject(calls, this.formatting, this.jsonSettings);
                writer.Write(contents);
            }
        }

        /// <summary>
        /// Reads calls from a file.
        /// </summary>
        /// <param name="fileStream">The stream that reads from the file.</param>
        /// <returns>The deserialized calls.</returns>
        protected override IEnumerable<RecordedCall> ReadFromStream(FileStream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                return JsonConvert.DeserializeObject<RecordedCall[]>(reader.ReadToEnd());
            }
        }
    }
}
