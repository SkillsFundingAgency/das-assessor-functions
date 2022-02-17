namespace SFA.DAS.Assessor.Functions.Infrastructure
{
    public static class QueueNames
    {
        public const string RefreshIlrs = "sfa-das-assessor-refresh-ilrs";
        
        public const string CertificatePrintStatusUpdate = "sfa-das-assessor-certificate-print-status-update";
        public const string CertificatePrintStatusUpdateErrors = "sfa-das-assessor-certificate-print-status-update-error";
        public const string UpdateLearnersInfo = "sfa-das-assessor-update-learners";
        public const string StartUpdateLearnersInfo = "sfa-das-assessor-start-update-learners";
    }
}
