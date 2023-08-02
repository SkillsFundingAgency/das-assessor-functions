using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.Data;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualFunctions
    {
        private readonly IOfqualDownloadsBlobFileTransferClient _blobFileTransferClient;
        private readonly IAssessorServiceRepository _assessorServiceRepository;

        public OfqualFunctions(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, IAssessorServiceRepository assessorServiceRepository)
        {
            _blobFileTransferClient = blobFileTransferClient;
            _assessorServiceRepository = assessorServiceRepository;
        }
        
        [FunctionName("OfqualImport")]
        public static async Task<HttpResponseMessage> OfqualImportHttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OfqualImportOrchestrator", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName("OfqualImportOrchestrator")]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var fileName = await context.CallActivityAsync<string>("DownloadFile", $"https://register.ofqual.gov.uk/Home/Download?category=Organisations");
            var fileDetails = await context.CallActivityAsync<string>("ReadFile", fileName);
            await context.CallActivityAsync("UpdateDatabase", fileDetails);
        }

        [FunctionName("DownloadFile")]
        public async Task<string> DownloadFile([ActivityTrigger] string fileName, ILogger log)
        {
            log.LogInformation($"Successfully executed DownloadFile for {fileName} activity.");

            var outputFileName = $"Downloads/MyFile{DateTime.Now.ToString("ddMMyyyy_HHmmss")}";
            using (HttpClient client = new HttpClient())
            { 
                var fileContents = await client.GetStringAsync(fileName);
                await _blobFileTransferClient.UploadFile(fileContents, outputFileName + ".txt");
            }
            
            return outputFileName;
        }

        [FunctionName("ReadFile")]
        public async Task<string> ReadFile([ActivityTrigger] string fileName, ILogger log)
        {
            log.LogInformation($"Successfully executed ReadFile for {fileName} activity.");

            var fileContents = await _blobFileTransferClient.DownloadFile(fileName + ".txt");

            return $"{fileName}_{fileContents.Length}.txt";
        }

        [FunctionName("UpdateDatabase")]
        public async Task UpdateDatabase([ActivityTrigger] string fileName, ILogger log)
        {
            log.LogInformation($"Successfully executed UpdateDatabase for {fileName} activity.");
            await _assessorServiceRepository.InsertSearchLogsDataBase(fileName + "_received_by_UpdateDatabase");
        }
    }
}