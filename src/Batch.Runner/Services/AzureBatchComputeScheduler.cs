using System;
using System.Collections.Generic;
using System.Linq;
using Batch.Runner.Domain;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Runtime;

namespace Batch.Runner.Services
{
    public class AzureBatchComputeScheduler : IComputeScheduler
    {
        static readonly string[] Locations = {
            // Not all Azure locations are listed; please add the locations you want to use here
            "North Europe",
            "West Europe",
            "South Central US",
            "West US",
            "North Central US",
            "East US",
            "Southeast Asia",
            "East Asia"
        };

        private readonly BatchSharedKeyCredentials _credentials;

        private readonly IDictionary<ComputeJob, string> _jobIdToPoolId = new Dictionary<ComputeJob, string>();
        private readonly IDictionary<string, List<ComputeJob>> _pools = new Dictionary<string, List<ComputeJob>>();

        public AzureBatchComputeScheduler(IApplicationEnvironment applicationEnvironment)
        {
            var configuration = new ConfigurationBuilder(applicationEnvironment.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables()
                .Build();

            _credentials = new BatchSharedKeyCredentials(
                configuration.GetSection("AppSettings:BatchAccountUrl").Value,
                configuration.GetSection("AppSettings:BatchAccountName").Value,
                configuration.GetSection("AppSettings:BatchKey").Value);
        }

        public void BeginJob(ComputeJob computeJob)
        {
            var poolId = CreatePoolIfRequired();

            using (var client = BatchClient.Open(_credentials))
            {
                var cloudJob = client.JobOperations.ListJobs().SingleOrDefault(j => j.Id == computeJob.Id);

                if (cloudJob == null)
                {
                    cloudJob = client.JobOperations.CreateJob();
                    cloudJob.Id = computeJob.Id;
                    cloudJob.PoolInformation = new PoolInformation { PoolId = poolId };
                    cloudJob.Commit();
                }

                _jobIdToPoolId.Add(computeJob, poolId);
                _pools[poolId].Add(computeJob);

                AddTasks(client, cloudJob, computeJob.Id, computeJob.ComputeTasks);
            }
        }

        public void DeleteJob(ComputeJob computeJob)
        {
            using (var client = BatchClient.Open(_credentials))
            {
                client.JobOperations.DeleteJob(computeJob.Id);

                var poolId = _jobIdToPoolId[computeJob];
                _jobIdToPoolId.Remove(computeJob);

                DeletePoolIfRequired(poolId);
            }
        }

        public void AddTask(ComputeJob computeJob, IComputeTask computeTask)
        {
            using (var client = BatchClient.Open(_credentials))
            {
                var cloudJob = client.JobOperations.GetJob(computeJob.Id);

                AddTasks(client, cloudJob, computeJob.Id, new[] { computeTask });
            }
        }

        string CreatePoolIfRequired()
        {
            var poolId = "testpool1";

            using (var client = BatchClient.Open(_credentials))
            {
                var pool = client.PoolOperations.ListPools().SingleOrDefault(t => t.Id == poolId);

                if (pool == null)
                {
                    pool = client.PoolOperations.CreatePool(poolId, "3", "small", 3);
                    pool.Commit();
                }

                _pools.Add(poolId, new List<ComputeJob>());
            }

            return poolId;
        }

        void DeletePoolIfRequired(string poolId)
        {
            if (!_pools.ContainsKey(poolId))
            {
                return;
            }

            if (_pools[poolId].Count != 0)
            {
                return;
            }

            using (var client = BatchClient.Open(_credentials))
            {
                client.PoolOperations.DeletePool(poolId);
            }
        }

        static void AddTasks(BatchClient client, CloudJob cloudJob, string jobId, IEnumerable<IComputeTask> computeTasks)
        {
            foreach (var computeTask in computeTasks)
            {
                var definition = computeTask.Definition;
                var executable = new ResourceFile($"{definition.StorageUri}/{definition.ExecutableName}", definition.ExecutableName);
                var resources = definition.Resources.Select(resource => new ResourceFile($"{definition.StorageUri}/{resource}", resource));
                var inputs = computeTask.Inputs.Select(input => new ResourceFile($"{definition.StorageUri}/{input}", input));

                var resourceFiles = new List<ResourceFile> { executable };
                resourceFiles.AddRange(resources);
                resourceFiles.AddRange(inputs);

                var task = client.JobOperations.ListTasks(jobId).SingleOrDefault(t => t.Id == computeTask.Id);

                if (task == null)
                {
                    task = new CloudTask(computeTask.Id, computeTask.CommandLine)
                    {
                        ResourceFiles = resourceFiles
                    };

                    cloudJob.AddTask(task);
                    cloudJob.Commit();
                    cloudJob.Refresh();
                }
            }

            client.Utilities.CreateTaskStateMonitor().WaitAll(cloudJob.ListTasks(), TaskState.Completed, new TimeSpan(0, 30, 0));
        }
    }
}