using Microsoft.Extensions.Logging;
using SFA.DAS.Assessor.Functions.ApiClient.Types;
using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Extensions
{
    public static class CertificateReponseExtensions
    {
        public static List<CertificateResponse> Sanitise(this List<CertificateResponse> certificateResponses, ILogger logger)
        {
            var sanitisedCertificateResponse = new List<CertificateResponse>();

            foreach (var certificateResponse in certificateResponses)
            {
                var errorFlag = false;

                logger.Log(LogLevel.Information, $"Sanitising Certificate - {certificateResponse.CertificateReference} ...");

                var certificateData = certificateResponse.CertificateData;
                if (string.IsNullOrEmpty(certificateData.ContactAddLine1))
                {
                    errorFlag = true;
                }

                if (string.IsNullOrEmpty(certificateData.ContactPostCode))
                {
                    errorFlag = true;
                }

                if (errorFlag)
                {
                    if (!string.IsNullOrEmpty(certificateData.LearnerGivenNames)
                        && !string.IsNullOrEmpty(certificateData.LearnerFamilyName))
                    {
                        logger.Log(LogLevel.Information, $"Unprintable Certificate -> Given Names = {certificateData.LearnerGivenNames} Family Name = {certificateData.LearnerFamilyName} - Cannot be processed");
                    }
                    else
                    {
                        logger.Log(LogLevel.Information, $"Unprintable Certificate - Cannot be processed");
                    }
                }
                else
                {
                    sanitisedCertificateResponse.Add(certificateResponse);
                }
            }

            return sanitisedCertificateResponse;
        }
    }
}
