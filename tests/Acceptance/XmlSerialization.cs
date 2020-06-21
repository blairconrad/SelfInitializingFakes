namespace SelfInitializingFakes.Tests.Acceptance
{
    using System;
    using System.IO;

    public class XmlSerialization : TypeSerializationTestBase
    {
        protected override IRecordedCallRepository CreateRepository()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
            return new XmlFileRecordedCallRepository(path);
        }
    }
}
