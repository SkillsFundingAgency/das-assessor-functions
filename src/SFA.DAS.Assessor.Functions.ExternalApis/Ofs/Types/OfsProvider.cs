using SFA.DAS.Assessor.Functions.Domain.Entities.Ofs;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Types
{
    public class OfsProvider
    {
        public string Ukprn { get; set; }
        public string RegistrationStatus { get; set; }
        public string HighestLevelOfDegreeAwardingPowers { get; set; }

        public static explicit operator OfsOrganisation(OfsProvider provider)
        {
            return new OfsOrganisation(
                int.Parse(provider.Ukprn),
                provider.RegistrationStatus,
                provider.HighestLevelOfDegreeAwardingPowers
            );
        }
    }
}
