namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    public class InMemoryRecordedCallRepository : IRecordedCallRepository
    {
        private IEnumerable<RecordedCall>? recordedCalls;

        public IEnumerable<RecordedCall>? Load() => this.recordedCalls;

        public void Save(IEnumerable<RecordedCall> calls)
        {
            this.recordedCalls = calls.ToList();
        }
    }
}
