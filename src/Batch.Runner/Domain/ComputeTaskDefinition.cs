using System;
using System.Collections.Generic;

namespace Batch.Runner.Domain
{
    public class ComputeTaskDefinition
    {
        public ComputeTaskDefinition()
        {
            Resources = new List<string> { "Microsoft.WindowsAzure.Storage.dll" };
        }

        public string BinaryFilePath { get; set; }

        public string DataFilePath { get; set; }
        
        public Uri StorageUri { get; set; }

        public string ExecutableName { get; set; }

        public IEnumerable<string> Resources { get; private set; }
    }
}