using System;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Types
{
    public class RefreshIlrsProviderMessage : IEquatable<RefreshIlrsProviderMessage>
    {
        public int Ukprn { get; set; }
        public string Source { get; set; }
        public int LearnerPageNumber { get; set; }

        public override int GetHashCode() =>
            (Ukprn, Source, LearnerPageNumber)
                .GetHashCode();

        public override bool Equals(object obj) =>
            obj is RefreshIlrsProviderMessage other &&
                Equals(other);

        public bool Equals(RefreshIlrsProviderMessage other) =>
            (Ukprn, Source, LearnerPageNumber)
                .Equals((other.Ukprn, other.Source, other.LearnerPageNumber));
    }
}
