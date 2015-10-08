using System;
using System.Collections.Generic;
using System.IO;
using Batch.Runner.Domain;
using Batch.Runner.Services;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Batch.Runner
{
    public class Program
    {
        readonly IApplicationEnvironment _applicationEnvironment;
        readonly IComputeScheduler _scheduler;
        readonly IConfigurationRoot _configuration;

        public Program(IApplicationEnvironment applicationEnvironment)
        {
            _applicationEnvironment = applicationEnvironment;

            // TODO: IoC
            _scheduler = new AzureBatchComputeScheduler(applicationEnvironment);
            _configuration = new ConfigurationBuilder(applicationEnvironment.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddUserSecrets()
                .AddEnvironmentVariables()
                .Build();
        }

        public void Main(string[] args)
        {
            var binaryFilePath = Path.Combine(Path.GetDirectoryName(_applicationEnvironment.ApplicationBasePath), "SimpleTaskRunner\\bin\\debug");
            var dataFilePath = Path.Combine(_applicationEnvironment.ApplicationBasePath, "Data");
            var container = GetCloudBlobContainer(_configuration.GetSection("AppSettings:StorageConnectionString").Value, "batch");

            var taskDefiniton = new ComputeTaskDefinition
            {
                BinaryFilePath = binaryFilePath,
                DataFilePath = dataFilePath,
                StorageUri = container.Uri,
                ExecutableName = "SimpleTaskRunner.exe"
            };

            var computeTasks = new List<IComputeTask>
            {
                new ComputeTask("task-1", taskDefiniton).AddArguments(new InputComputeTaskArgument("taskdata1.txt"), new SimpleComputeTaskArgument("3")),
                new ComputeTask("task-2", taskDefiniton).AddArguments(new InputComputeTaskArgument("taskdata2.txt"), new SimpleComputeTaskArgument("3")),
                new ComputeTask("task-3", taskDefiniton).AddArguments(new InputComputeTaskArgument("taskdata3.txt"), new SimpleComputeTaskArgument("3"))
            }.ToArray();

            var computeJob = new ComputeJob("job-1") { ComputeTasks = computeTasks };

            Console.WriteLine("Prepared the files. Press Enter to continue.");
            Console.ReadLine();

            var jobInstance = new JobInstance(_scheduler, container);
            jobInstance.OnScheduleRequested(computeJob);
            jobInstance.PrepareJob();
            jobInstance.Execute();
            jobInstance.ProcessResults();
            //jobInstance.Cleanup();
        }

        static CloudBlobContainer GetCloudBlobContainer(string connectionString, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(containerName);

            // Set permissions on the container
            var containerPermissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            container.SetPermissions(containerPermissions);

            return container;
        }
    }
}
