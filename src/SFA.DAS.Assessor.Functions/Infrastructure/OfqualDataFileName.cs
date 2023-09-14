using System;
using SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual;

namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    internal static class OfqualDataFileName
    {
        internal static string CreateForFileType(OfqualDataType fileType)
        {
            return $"{fileType}_export_{DateTime.UtcNow.ToString("yyyyMMdd")}.csv";
        }
    }
}
