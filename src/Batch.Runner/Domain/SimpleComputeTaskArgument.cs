namespace Batch.Runner.Domain
{
    public class SimpleComputeTaskArgument : ComputeTaskArgument
    {
        public SimpleComputeTaskArgument(string value) : base(value)
        {
        }

        public override string ToString()
        {
            return Value;
        }
    }
}