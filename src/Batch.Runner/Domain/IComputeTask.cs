using System.Collections.Generic;

namespace Batch.Runner.Domain
{
    public interface IComputeTask
    {
        string Id { get; }
        ComputeTaskDefinition Definition { get; }
        IEnumerable<string> Inputs { get; }
        string CommandLine { get; }
        void AddInput(string inputFileName);
        IComputeTask AddArguments(params ComputeTaskArgument[] arguments);
    }
}