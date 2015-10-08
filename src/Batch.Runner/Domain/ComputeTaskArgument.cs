namespace Batch.Runner.Domain
{
    public abstract class ComputeTaskArgument
    {
        protected ComputeTaskArgument(string value)
        {
            Value = value;
        }

        protected string Value { get; }

        protected IComputeTask Parent { get; private set; }

        public virtual void Initialise(IComputeTask computeTask)
        {
            Parent = computeTask;
        }
    }
}