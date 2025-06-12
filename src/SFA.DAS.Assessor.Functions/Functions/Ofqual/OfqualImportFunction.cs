using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualImportFunction
    {
        [FunctionName("OfqualImport")]
        public static async Task Run(
            [TimerTrigger("%FunctionsOptions:OfqualImportOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    log.LogInformation("OfqualImport timer trigger has started later than scheduled");
                }

                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync(nameof(RunOfqualImportOrchestrator), null);

                log.LogInformation($"Started OfqualImport with ID = '{instanceId}'.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"OfqualImport failed. Exception message: {ex.Message}");
                throw;
            }
        }

        [FunctionName(nameof(RunOfqualImportOrchestrator))]
        public async Task<int> RunOfqualImportOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            ILogger replaySafeLogger = context.CreateReplaySafeLogger(log);

            var parallelTasks = new List<Task>
            {
                DoOrganisationsTasks(context),
                DoQualificationsTasks(context)
            };

            await Task.WhenAll(parallelTasks);

            replaySafeLogger.LogInformation("Loading Ofqual Standards for Ofqual Organisations using data in staging tables.");

            int standardsLoaded = await context.CallActivityAsync<int>(nameof(OfqualStandardsLoader.LoadStandards), null);

            replaySafeLogger.LogInformation($"Ofqual import complete. {standardsLoaded} Ofqual standards were loaded for Ofqual organisations.");

            return standardsLoaded;
        }

        private async Task DoOrganisationsTasks(IDurableOrchestrationContext context)
        {
            string organisationsDataFilePath = await context.CallActivityAsync<string>(nameof(OrganisationsDownloader.DownloadOrganisationsData), null);
            var ofqualOrganisationData = await context.CallActivityAsync<IEnumerable<OfqualOrganisation>>(nameof(OfqualDataReader.ReadOrganisationsData), organisationsDataFilePath);
            await context.CallActivityAsync<int>(nameof(OrganisationsStager.InsertOrganisationsDataIntoStaging), ofqualOrganisationData);
            await context.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), organisationsDataFilePath);
        }

        private async Task DoQualificationsTasks(IDurableOrchestrationContext context)
        {
            string qualificationsDataFilePath = await context.CallActivityAsync<string>(nameof(QualificationsDownloader.DownloadQualificationsData), null);
            var ofqualQualificationData = await context.CallActivityAsync<IEnumerable<OfqualStandard>>(nameof(OfqualDataReader.ReadQualificationsData), qualificationsDataFilePath);
            await context.CallActivityAsync<int>(nameof(QualificationsStager.InsertQualificationsDataIntoStaging), ofqualQualificationData);
            await context.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), qualificationsDataFilePath);
        }
    }
}
