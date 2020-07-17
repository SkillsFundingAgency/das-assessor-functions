﻿using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Extensions
{
    public static class CertificateResponseExtensions
    {
        public static List<CertificateToBePrintedSummary> Sanitise(this List<CertificateToBePrintedSummary> certificates, ILogger logger)
        {
            var sanitisedCertificates = new List<CertificateToBePrintedSummary>();

            foreach (var certificate in certificates)
            {
                var errorFlag = false;

                logger.Log(LogLevel.Information, $"Sanitising Certificate - {certificate.CertificateReference} ...");

                if (string.IsNullOrEmpty(certificate.ContactAddLine1))
                {
                    errorFlag = true;
                }

                if (string.IsNullOrEmpty(certificate.ContactPostCode))
                {
                    errorFlag = true;
                }

                if (errorFlag)
                {
                    if (!string.IsNullOrEmpty(certificate.LearnerGivenNames)
                        && !string.IsNullOrEmpty(certificate.LearnerFamilyName))
                    {
                        logger.Log(LogLevel.Information, $"Unprintable Certificate -> Given Names = {certificate.LearnerGivenNames} Family Name = {certificate.LearnerFamilyName} - Cannot be processed");
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, $"Unprintable Certificate - Cannot be processed");
                    }
                }
                else
                {
                    sanitisedCertificates.Add(certificate);
                }
            }

            return sanitisedCertificates;
        }
    }
}