using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualImportFunction
    {
        private readonly ILogger<OfqualImportFunction> _logger;

        public OfqualImportFunction(ILogger<OfqualImportFunction> logger)
        {
            _logger = logger;
        }

        [Function("OfqualImport")]
        public async Task Run(
            [TimerTrigger("%FunctionsOptions:OfqualImportOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer,
            [DurableClient] DurableTaskClient starter)
        {
            try
            {
                if (myTimer.IsPastDue)
                {
                    _logger.LogInformation("OfqualImport timer trigger has started later than scheduled");
                }

                string instanceId = await starter.ScheduleNewOrchestrationInstanceAsync(nameof(RunOfqualImportOrchestrator), null);

                _logger.LogInformation($"Started OfqualImport with ID = '{instanceId}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OfqualImport failed. Exception message: {ex.Message}");
                throw;
            }
        }

        [Function(nameof(RunOfqualImportOrchestrator))]
        public async Task<int> RunOfqualImportOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {

            var parallelTasks = new List<Task>
            {
                DoOrganisationsTasks(context),
                DoQualificationsTasks(context)
            };

            await Task.WhenAll(parallelTasks);

            _logger.LogInformation("Loading Ofqual Standards for Ofqual Organisations using data in staging tables.");

            int standardsLoaded = await context.CallActivityAsync<int>(nameof(OfqualStandardsLoader.LoadStandards), null);

            _logger.LogInformation($"Ofqual import complete. {standardsLoaded} Ofqual standards were loaded for Ofqual organisations.");

            return standardsLoaded;
        }

        private async Task DoOrganisationsTasks(TaskOrchestrationContext context)
        {
            string organisationsDataFilePath = await context.CallActivityAsync<string>(nameof(OrganisationsDownloader.DownloadOrganisationsData), null);
            var ofqualOrganisationData = await context.CallActivityAsync<IEnumerable<OfqualOrganisation>>(nameof(OfqualDataReader.ReadOrganisationsData), organisationsDataFilePath);
            await context.CallActivityAsync<int>(nameof(OrganisationsStager.InsertOrganisationsDataIntoStaging), ofqualOrganisationData);
            await context.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), organisationsDataFilePath);
        }

        private async Task DoQualificationsTasks(TaskOrchestrationContext context)
        {
            string qualificationsDataFilePath = await context.CallActivityAsync<string>(nameof(QualificationsDownloader.DownloadQualificationsData), null);
            var ofqualQualificationData = await context.CallActivityAsync<IEnumerable<OfqualStandard>>(nameof(OfqualDataReader.ReadQualificationsData), qualificationsDataFilePath);
            await context.CallActivityAsync<int>(nameof(QualificationsStager.InsertQualificationsDataIntoStaging), ofqualQualificationData);
            await context.CallActivityAsync(nameof(OfqualFileMover.MoveOfqualFileToProcessed), qualificationsDataFilePath);
        }
    }
}
