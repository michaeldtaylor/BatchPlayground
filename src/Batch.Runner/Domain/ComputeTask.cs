using System.Collections.Generic;

namespace Batch.Runner.Domain
{
    public class ComputeTask : IComputeTask
    {
        readonly IList<string> _inputs = new List<string>();
        readonly IList<ComputeTaskArgument> _arguments = new List<ComputeTaskArgument>();

        public ComputeTask(string id, ComputeTaskDefinition definition)
        {
            Id = id;
            Definition = definition;
        }

        public string Id { get; }

        public ComputeTaskDefinition Definition { get; }

        public IEnumerable<string> Inputs => _inputs;

        public string CommandLine => $"{Definition.ExecutableName} {string.Join(" ", _arguments)}";

        public void AddInput(string inputFileName)
        {
            _inputs.Add(inputFileName);
        }

        public IComputeTask AddArguments(params ComputeTaskArgument[] arguments)
        {
            foreach (var argument in arguments)
            {
                argument.Initialise(this);

                _arguments.Add(argument);
            }

            return this;
        }
    }
}
