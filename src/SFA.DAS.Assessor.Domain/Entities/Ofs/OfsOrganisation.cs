using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofs
{
    [ExcludeFromCodeCoverage]
    public record OfsOrganisation(
        int Ukprn,
        string RegistrationStatus,
        string HighestLevelOfDegreeAwardingPowers
    );
}
