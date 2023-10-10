using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual
{
    [ExcludeFromCodeCoverage]
    public record OfqualOrganisation(
        string? RecognitionNumber,
        string? Name,
        string? LegalName,
        string? Acronym,
        string? Email,
        string? Website,
        string? HeadOfficeAddressLine1,
        string? HeadOfficeAddressLine2,
        string? HeadOfficeAddressTown,
        string? HeadOfficeAddressCounty,
        string? HeadOfficeAddressPostcode,
        string? HeadOfficeAddressCountry,
        string? HeadOfficeAddressTelephone,
        string? OfqualStatus,
        DateTime? OfqualRecognisedFrom,
        DateTime? OfqualRecognisedTo
    ) : IOfqualRecord;
}
