using Batch.Runner.Domain;

namespace Batch.Runner.Services
{
    public interface IComputeScheduler
    {
        void BeginJob(ComputeJob job);
        void DeleteJob(ComputeJob job);
        void AddTask(ComputeJob job, IComputeTask computeTask);
    }
}
