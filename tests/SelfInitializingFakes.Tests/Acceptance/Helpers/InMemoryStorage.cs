namespace SelfInitializingFakes.Tests.Acceptance.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    public class InMemoryStorage : ICallDataRepository
    {
        private IEnumerable<ICallData> recordedCalls;

        public IEnumerable<ICallData> Load()
        {
            return this.recordedCalls;
        }

        public void Save(IEnumerable<ICallData> calls)
        {
            this.recordedCalls = calls.ToList();
        }
    }
}
