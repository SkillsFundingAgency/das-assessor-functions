using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual
{
    [ExcludeFromCodeCoverage]
    public record OfqualStandard(
        string RecognitionNumber,
        string IFateReferenceNumber,
        DateTime OperationalStartDate,
        DateTime? OperationalEndDate = null
    ) : IOfqualRecord;
}
