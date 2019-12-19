using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Assessor.Functions.Domain;
using SFA.DAS.Assessor.Functions.ExternalApis.DataCollection;
using SFA.DAS.Assessor.Functions.ExternalApis.Exceptions;

namespace SFA.DAS.Assessor.Functions.Epao
{
    public class EpaoDataSyncEnqueueProviders
    {
        private readonly IDataCollectionServiceApiClient _dataCollectionServiceApiClient;
        private readonly IConfiguration _configuration;

        public EpaoDataSyncEnqueueProviders(IDataCollectionServiceApiClient dataCollectionServiceApiClient, IConfiguration configuration)
        {
            _dataCollectionServiceApiClient = dataCollectionServiceApiClient;
            _configuration = configuration;
        }

        [FunctionName("EpaoDataSyncEnqueueProviders")]
        public async Task Run([TimerTrigger("0 0 7 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("Epao data sync enqueue providers timer trigger is running later than scheduled");
                }

                // TODO: extract this into a service; encapulate queue for mocking in unit tests

                log.LogInformation($"Epao data sync enqueue providers function started");
                log.LogInformation($"Using data collection api base address: {_dataCollectionServiceApiClient.BaseAddress()}");

                var queue = GetQueue("epao-data-sync-providers");

                var lastRunDateTime = new DateTime(2019, 9, 1); // TODO: read this from assessor config table
                var nextRunDateTime = DateTime.Now;
                
                var sources = new List<string> { "1920" }; // TODO get a collection of applicable academic years from data collection for last run date
                foreach (var source in sources)
                {
                    try
                    {
                        var providersQueued = false;
                        while (!providersQueued)
                        {
                            try
                            {
                                await QueueProviders(queue, source, lastRunDateTime);

                                // TODO: update the last run date in assessor config table

                                providersQueued = true;
                            }
                            catch (PagingInfoChangedException)
                            {
                                // the queue process will be restarted when providers have changed whilst queueing but the 
                                // next run date will be reset to avoid duplicating them on the next run
                                nextRunDateTime = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Epao data sync enqueue providers function failed");
                    }
                }

                log.LogInformation("Epao data sync enqueue providers function completed");
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Epao data sync enqueue providers function failed");
            }
        }

        private async Task QueueProviders(CloudQueue queue, string source, DateTime lastRunDateTime)
        {
            const int pageSize = 2; // TODO: increase to large value e.g. 3000 for production to avoid paging if possible
            
            var providersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, pageNumber: 1);
            if (providersPage != null)
            {
                do
                {
                    foreach (var providerUkprn in providersPage.Providers)
                    {
                        var message = new EpaoDataSyncProviderMessage
                        {
                            Ukprn = providerUkprn,
                            Source = source
                        };

                        var jsonMessage = JsonConvert.SerializeObject(message);
                        await queue.AddMessageAsync(new CloudQueueMessage(jsonMessage));
                    }

                    var nextProvidersPage = await _dataCollectionServiceApiClient.GetProviders(source, lastRunDateTime, pageSize, providersPage.PagingInfo.PageNumber + 1);
                    if (nextProvidersPage != null)
                    {
                        if (nextProvidersPage.PagingInfo.TotalItems != providersPage.PagingInfo.TotalItems || nextProvidersPage.PagingInfo.TotalPages != providersPage.PagingInfo.TotalPages)
                        {
                            // if the total number of items or pages has changed then the process will need to be restarted to 
                            // avoid skipping any updated providers on earlier pages
                            throw new PagingInfoChangedException();
                        }
                    }

                    providersPage = nextProvidersPage;
                }
                while (providersPage != null && providersPage.PagingInfo.PageNumber <= providersPage.PagingInfo.TotalPages);
            }
        }

        public CloudQueue GetQueue(string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration["ConfigurationStorageConnectionString"]);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);

            return queue;
        }
    }
}
