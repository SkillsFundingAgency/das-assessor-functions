using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual
{
    [ExcludeFromCodeCoverage]
    public record class OfqualStandard : IOfqualRecord
    {
        public string RecognitionNumber { get; init; }
        public DateTime OperationalStartDate { get; init; }
        public DateTime? OperationalEndDate { get; init; }
        public string IFateReferenceNumber { get; init; }

        public OfqualStandard(string recognitionNumber, string iFateReferenceNumber, DateTime operationalStartDate, DateTime? operationalEndDate = null)
        {
            RecognitionNumber = recognitionNumber;
            IFateReferenceNumber = iFateReferenceNumber;
            OperationalStartDate = operationalStartDate;
            OperationalEndDate = operationalEndDate;
        }
    }
}
