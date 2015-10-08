namespace Batch.Runner.Domain
{
    public class InputComputeTaskArgument : ComputeTaskArgument
    {
        public InputComputeTaskArgument(string value) : base(value)
        {
        }

        public override void Initialise(IComputeTask computeTask)
        {
            base.Initialise(computeTask);

            Parent.AddInput(Value);
        }

        public override string ToString()
        {
            return $"{Parent.Definition.StorageUri}/{Value}";
        }
    }
}