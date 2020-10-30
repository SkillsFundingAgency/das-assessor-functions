namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateBatchLogAddCertificatesReadyToPrintRequest
    {
        public int BatchNumber { get; set; }
        public int MaxCertificatesToBeAdded { get; set; }
    }
}
