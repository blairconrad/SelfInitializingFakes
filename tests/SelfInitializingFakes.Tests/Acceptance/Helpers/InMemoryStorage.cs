namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    public class InMemoryStorage : IRecordedCallRepository
    {
        private IEnumerable<RecordedCall> recordedCalls;

        public IEnumerable<RecordedCall> Load()
        {
            return this.recordedCalls;
        }

        public void Save(IEnumerable<RecordedCall> calls)
        {
            this.recordedCalls = calls.ToList();
        }
    }
}
