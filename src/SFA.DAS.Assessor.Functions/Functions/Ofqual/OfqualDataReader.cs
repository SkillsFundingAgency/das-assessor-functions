using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CsvHelper;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;
using SFA.DAS.Assessor.Functions.Domain.OfqualImport.Interfaces;

namespace SFA.DAS.Assessor.Functions.Functions.Ofqual
{
    public class OfqualDataReader
    {
        private readonly IOfqualDownloadsBlobFileTransferClient _blobFileTransferClient;
        private readonly ILogger<OfqualDataReader> _logger;

        public OfqualDataReader(IOfqualDownloadsBlobFileTransferClient blobFileTransferClient, ILogger<OfqualDataReader> logger)
        {
            _blobFileTransferClient = blobFileTransferClient;
            _logger = logger;
        }

        [Function(nameof(ReadOrganisationsData))]
        public async Task<IEnumerable<OfqualOrganisation>> ReadOrganisationsData([ActivityTrigger] string filePath)
        {
            if (!await FileExists(filePath))
            {
                _logger.LogError($"Failed to find Organisations data file at {filePath}.");
                throw new FileNotFoundException($"Could not find the Organisations data file at {filePath}");
            }

            _logger.LogInformation($"Organisations data file found at {filePath}. Reading.");

            string organisationsData = await _blobFileTransferClient.DownloadFile(filePath);
            using var stringReader = new StringReader(organisationsData);
            using var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);

            var records = new List<OfqualOrganisation>();
            csvReader.Read();
            csvReader.ReadHeader();

            while (csvReader.Read())
            {
                var record = new OfqualOrganisation(
                    csvReader.GetField<string>("Recognition Number"),
                    csvReader.GetField<string>("Name"),
                    csvReader.GetField<string>("Legal Name"),
                    csvReader.GetField<string>("Acronym"),
                    csvReader.GetField<string>("Email"),
                    csvReader.GetField<string>("Website"),
                    csvReader.GetField<string>("Head Office Address Line 1"),
                    csvReader.GetField<string>("Head Office Address Line 2"),
                    csvReader.GetField<string>("Head Office Address Town/City"),
                    csvReader.GetField<string>("Head Office Address County"),
                    csvReader.GetField<string>("Head Office Address Postcode"),
                    csvReader.GetField<string>("Head Office Address County"),
                    csvReader.GetField<string>("Head Office Address Telephone Number"),
                    csvReader.GetField<string>("Ofqual Status"),
                    csvReader.TryGetField<DateTime?>("Ofqual Recognised From", out var ofQualFromDate) ? ofQualFromDate : null,
                    csvReader.TryGetField<DateTime?>("Ofqual Recognised To", out var ofQualToDate) ? ofQualToDate : null
                );
                records.Add(record);
            }

            return records;
        }

        [Function(nameof(ReadQualificationsData))]
        public async Task<IEnumerable<OfqualStandard>> ReadQualificationsData([ActivityTrigger] string filePath)
        {
            if (!await FileExists(filePath))
            {
                _logger.LogError($"Failed to find Qualifications data file at {filePath}.");
                throw new FileNotFoundException($"Could not find the Qualifications data file at {filePath}");
            }

            _logger.LogInformation($"Qualifications data file found at {filePath}. Reading.");

            string qualificationsData = await _blobFileTransferClient.DownloadFile(filePath);
            using var stringReader = new StringReader(qualificationsData);
            using var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);

            var records = new List<OfqualStandard>();
            csvReader.Read();
            csvReader.ReadHeader();

            while (csvReader.Read())
            {
                var apprenticeshipStandardReferenceNumber = csvReader.GetField<string>("Apprenticeship Standard Reference Number");

                if (!string.IsNullOrWhiteSpace(apprenticeshipStandardReferenceNumber))
                {
                    var record = new OfqualStandard(
                    csvReader.GetField<string>("Owner Organisation Recognition Number"),
                    apprenticeshipStandardReferenceNumber,
                    csvReader.GetField<DateTime>("Operational Start Date"),
                    csvReader.TryGetField<DateTime?>("Operational End Date", out var operationalEndDate) ? operationalEndDate : null);

                    records.Add(record);
                }

            }

            return records;
        }

        private async Task<bool> FileExists(string filePath)
        {
            bool? fileExists = await _blobFileTransferClient.FileExists(filePath);
            if (!fileExists.Value)
            {
                return false;
            }

            return true;
        }
    }
}
