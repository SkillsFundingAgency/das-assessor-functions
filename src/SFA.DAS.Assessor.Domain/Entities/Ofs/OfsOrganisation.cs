namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofs
{
    public record class OfsOrganisation
    {
        public int Ukprn { get; init; }
        public string RegistrationStatus { get; init; }
        public string HighestLevelOfDegreeAwardingPowers { get; init; }

        public OfsOrganisation(int ukprn, string registrationStatus, string highestLevelOfDegreeAwardingPowers)
        {
            Ukprn = ukprn;
            RegistrationStatus = registrationStatus;
            HighestLevelOfDegreeAwardingPowers = highestLevelOfDegreeAwardingPowers;
        }
    }
}
