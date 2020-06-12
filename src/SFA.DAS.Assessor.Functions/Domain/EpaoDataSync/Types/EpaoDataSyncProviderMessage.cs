namespace SFA.DAS.Assessor.Functions.Domain.EpaoDataSync.Types
{
    public class EpaoDataSyncProviderMessage
    {
        public int Ukprn { get; set; }
        public string Source { get; set; }
        public int LearnerPageNumber { get; set; }

        public override bool Equals(object obj)
        {
            return obj is EpaoDataSyncProviderMessage other &&
                Ukprn.Equals(other.Ukprn) &&
                Source.Equals(other.Source) &&
                LearnerPageNumber.Equals(other.LearnerPageNumber);
        }
    }
}
