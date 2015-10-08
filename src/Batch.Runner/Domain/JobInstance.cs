using System;
using System.Collections.Generic;
using System.IO;
using Batch.Runner.Services;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Batch.Runner.Domain
{
    public class JobInstance
    {
        readonly IList<Type> _definitionTypes = new List<Type>();

        readonly IComputeScheduler _computeScheduler;
        readonly CloudBlobContainer _container;

        private ComputeJob _computeJob;

        public JobInstance(IComputeScheduler computeScheduler, CloudBlobContainer container)
        {
            _computeScheduler = computeScheduler;
            _container = container;
        }

        public void OnScheduleRequested(ComputeJob computeJob)
        {
            _computeJob = computeJob;
        }

        public void PrepareJob()
        {
            foreach (var computeTask in _computeJob.ComputeTasks)
            {
                var defintion = computeTask.Definition;
                
                if (!_definitionTypes.Contains(defintion.GetType()))
                {
                    // Upload Executable
                    var blobExeReference = _container.GetBlockBlobReference(defintion.ExecutableName);
                    blobExeReference.UploadFromFile(Path.Combine(defintion.BinaryFilePath, defintion.ExecutableName), FileMode.Open);

                    // Upload Resources
                    foreach (var resource in defintion.Resources)
                    {
                        var blobReference = _container.GetBlockBlobReference(resource);
                        blobReference.UploadFromFile(Path.Combine(defintion.BinaryFilePath, resource), FileMode.Open);
                    }

                    _definitionTypes.Add(defintion.GetType());
                }

                // Upload Inputs
                foreach (var input in computeTask.Inputs)
                {
                    var blobReference = _container.GetBlockBlobReference(input);
                    blobReference.UploadFromFile(Path.Combine(defintion.DataFilePath, input), FileMode.Open);
                }
            }
        }

        public void Execute()
        {
            _computeScheduler.BeginJob(_computeJob);
        }

        public void ProcessResults()
        {
        }

        public void Cleanup()
        {
            _computeScheduler.DeleteJob(_computeJob);
        }
    }
}
