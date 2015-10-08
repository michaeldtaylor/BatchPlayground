using System.Collections.Generic;

namespace Batch.Runner.Domain
{
    public class ComputeJob
    {
        public ComputeJob(string id)
        {
            Id = id;
        }

        public string Id { get; }
        
        public IEnumerable<IComputeTask> ComputeTasks { get; set; } 
    }
}
