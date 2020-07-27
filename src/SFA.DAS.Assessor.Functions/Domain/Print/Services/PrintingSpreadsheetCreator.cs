using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SFA.DAS.Assessor.Functions.Domain.Print.Extensions;
using SFA.DAS.Assessor.Functions.Domain.Print.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using SFA.DAS.Assessor.Functions.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Services
{
    public class PrintingSpreadsheetCreator : IPrintingSpreadsheetCreator
    {
        private readonly ILogger<PrintingSpreadsheetCreator> _logger;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly CertificateDetails _certificateDetails;
        private IEnumerable<CertificateToBePrintedSummary> _certificates;

        public PrintingSpreadsheetCreator(
            ILogger<PrintingSpreadsheetCreator> logger,
            IFileTransferClient fileTransferClient,
            IOptions<CertificateDetails> options)
        {
            _logger = logger;
            _fileTransferClient = fileTransferClient;
            _certificateDetails = options?.Value;
        }

        public void Create(int batchNumber, IEnumerable<CertificateToBePrintedSummary> certificates)
        {
            _logger.Log(LogLevel.Information, "Creating Excel Spreadsheet ....");

            var memoryStream = new MemoryStream();

            _certificates = certificates as CertificateToBePrintedSummary[] ?? certificates.ToArray();

            var utcNow = DateTime.UtcNow;
            var gmtNow = utcNow.UtcToTimeZoneTime(TimezoneNames.GmtStandardTimeZone);
            var fileName = $"IFA-Certificate-{gmtNow:MMyy}-{batchNumber.ToString().PadLeft(3, '0')}.xlsx";

            using (var package = new ExcelPackage(memoryStream))
            {
                CreateWorkBook(package);
                CreateWorkSheet(batchNumber, package);

                package.Save();

                _fileTransferClient.Send(memoryStream, fileName);

                memoryStream.Close();
            }

            _logger.Log(LogLevel.Information, "Completed Excel Spreadsheet ....");
        }

        private static void CreateWorkBook(ExcelPackage package)
        {
            var workbook = package.Workbook;
            workbook.Protection.LockWindows = true;
            workbook.Protection.LockStructure = true;
            workbook.View.ShowHorizontalScrollBar = true;
            workbook.View.ShowVerticalScrollBar = true;
            workbook.View.ShowSheetTabs = true;
        }

        private void CreateWorkSheet(int batchNumber, ExcelPackage package)
        {
            var utcNow = DateTime.UtcNow;
            var gmtNow = utcNow.UtcToTimeZoneTime(TimezoneNames.GmtStandardTimeZone);

            var monthYear = gmtNow.ToString("MMM yyyy");
            var worksheet = package.Workbook.Worksheets.Add(monthYear);

            CreateWorksheetDefaults(worksheet);
            CreateWorkbookProperties(package);

            CreateWorksheetHeader(batchNumber, worksheet);
            CreateWorksheetTableHeader(worksheet);

            CreateWorksheetData(worksheet);
        }

        private static void CreateWorksheetDefaults(ExcelWorksheet worksheet)
        {
            worksheet.Cells.Style.Font.Name = "Calibri";
            worksheet.View.PageLayoutView = false;
        }

        private static void CreateWorkbookProperties(ExcelPackage package)
        {
            package.Workbook.Properties.Title = "PrintFlow Prototype";
            package.Workbook.Properties.Author = "SFA";
            package.Workbook.Properties.Comments =
                "Printed Certificates information";
        }

        private static void CreateWorksheetTableHeader(ExcelWorksheet worksheet)
        {
            worksheet.Cells["K1:Q1"].Merge = true;
            worksheet.Cells["K1:Q1"].Value = "Employer Address Details";

            worksheet.Cells[2, 1].Value = "Achievement Date";
            worksheet.Cells[2, 2].Value = "Apprentice Name";
            worksheet.Cells[2, 3].Value = "Standard Title";
            worksheet.Cells[2, 4].Value = "Option";
            worksheet.Cells[2, 5].Value = "Level";
            worksheet.Cells[2, 6].Value = "achieving a";
            worksheet.Cells[2, 7].Value = "Grade";
            worksheet.Cells[2, 8].Value = "Certificate Number";
            worksheet.Cells[2, 9].Value = "Chair Name";
            worksheet.Cells[2, 10].Value = "Chair Title";
            worksheet.Cells[2, 11].Value = "Employer Contact";
            worksheet.Cells[2, 12].Value = "Employer Name";
            worksheet.Cells[2, 13].Value = "Department";
            worksheet.Cells[2, 14].Value = "Address Line 1";
            worksheet.Cells[2, 15].Value = "Address Line 2";
            worksheet.Cells[2, 16].Value = "Address Line 3";
            worksheet.Cells[2, 17].Value = "Address Line 4";
            worksheet.Cells[2, 18].Value = "Post Code";

            using (var range = worksheet.Cells[2, 1, 2, 18])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Font.Size = 16;
            }
        }

        private static void CreateWorksheetHeader(int batchNumber, ExcelWorksheet worksheet)
        {
            using (var range = worksheet.Cells[1, 1, 1, 18])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Color.SetColor(Color.Red);
                range.Style.Font.Size = 20;
            }

            var monthYear = DateTime.Today.ToString("MMMM yyyy");

            worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1:J1"].Value = monthYear + " Print Data - Batch " + batchNumber.ToString();
        }

        private void CreateWorksheetData(ExcelWorksheet worksheet)
        {
            var row = 3;

            foreach (var certificate in _certificates)
            {
                if (certificate.AchievementDate.HasValue)
                    worksheet.Cells[row, 1].Value = certificate.AchievementDate.Value.ToString("dd MMMM yyyy");

                worksheet.Cells[row, 2].Value = $"{certificate.LearnerGivenNames.ProperCase()} {certificate.LearnerFamilyName.ProperCase(true)}";

                if (certificate.StandardName != null)
                    worksheet.Cells[row, 3].Value = certificate.StandardName.ToUpper();

                if (!string.IsNullOrWhiteSpace(certificate.CourseOption))
                    worksheet.Cells[row, 4].Value = "(" + certificate.CourseOption.ToUpper() + "):";

                worksheet.Cells[row, 5].Value = $"Level {certificate.StandardLevel}".ToUpper();

                if (certificate.OverallGrade != null &&
                    !certificate.OverallGrade.ToLower().Contains("no grade awarded"))
                    worksheet.Cells[row, 6].Value = "Achieved grade ";

                if (certificate.OverallGrade != null &&
                    !certificate.OverallGrade.ToLower().Contains("no grade awarded"))
                    worksheet.Cells[row, 7].Value = certificate.OverallGrade.ToUpper();

                if (certificate.CertificateReference != null)
                    worksheet.Cells[row, 8].Value = certificate.CertificateReference.PadLeft(8, '0');

                worksheet.Cells[row, 9].Value = _certificateDetails.ChairName;
                worksheet.Cells[row, 10].Value = _certificateDetails.ChairTitle;

                if (certificate.ContactName != null)
                    worksheet.Cells[row, 11].Value = certificate.ContactName.Replace("\t", " ");

                if (certificate.ContactOrganisation != null)
                    worksheet.Cells[row, 12].Value = certificate.ContactOrganisation;

                if (certificate.Department != null)
                    worksheet.Cells[row, 13].Value = certificate.Department;

                if (certificate.ContactAddLine1 != null)
                    worksheet.Cells[row, 14].Value = certificate.ContactAddLine1;

                if (certificate.ContactAddLine2 != null)
                    worksheet.Cells[row, 15].Value = certificate.ContactAddLine2;

                if (certificate.ContactAddLine3 != null)
                    worksheet.Cells[row, 16].Value = certificate.ContactAddLine3;

                if (certificate.ContactAddLine4 != null)
                    worksheet.Cells[row, 17].Value = certificate.ContactAddLine4;

                if (certificate.ContactPostCode != null)
                    worksheet.Cells[row, 18].Value = certificate.ContactPostCode;

                row++;
            }
        }
    }
}
