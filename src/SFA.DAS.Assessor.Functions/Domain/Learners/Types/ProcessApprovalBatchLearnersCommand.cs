namespace SFA.DAS.Assessor.Functions.Domain.Learners.Types
{
    public class ProcessApprovalBatchLearnersCommand
    {
        public int BatchNumber { get; set; }

        public ProcessApprovalBatchLearnersCommand(int batchNumber)
        {
            BatchNumber = batchNumber;
        }
    }
}