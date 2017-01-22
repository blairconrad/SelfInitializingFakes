namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    public class InMemoryStorage : ICallDataRepository
    {
        private IEnumerable<CallData> recordedCalls;

        public IEnumerable<CallData> Load()
        {
            return this.recordedCalls;
        }

        public void Save(IEnumerable<CallData> calls)
        {
            this.recordedCalls = calls.ToList();
        }
    }
}
