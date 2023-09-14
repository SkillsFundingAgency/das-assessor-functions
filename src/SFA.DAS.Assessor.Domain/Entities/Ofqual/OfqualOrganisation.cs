using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Domain.Entities.Ofqual
{
    [ExcludeFromCodeCoverage]
    public record class OfqualOrganisation : IOfqualRecord
    {
        public string? RecognitionNumber { get; init; }
        public string? Name { get; init; }
        public string? LegalName { get; init; }
        public string? Acronym { get; init; }
        public string? Email { get; init; }
        public string? Website { get; init; }
        public string? HeadOfficeAddressLine1 { get; init; }
        public string? HeadOfficeAddressLine2 { get; init; }
        public string? HeadOfficeAddressTown { get; init; }
        public string? HeadOfficeAddressCounty { get; init; }
        public string? HeadOfficeAddressPostcode { get; init; }
        public string? HeadOfficeAddressCountry { get; init; }
        public string? HeadOfficeAddressTelephone { get; init; }
        public string? OfqualStatus { get; init; }
        public DateTime? OfqualRecognisedFrom { get; init; }
        public DateTime? OfqualRecognisedTo { get; init; }

        public OfqualOrganisation(string? recognitionNumber, string? name, string? legalName, string? acronym, string? email, string? website, string? headOfficeAddressLine1, string? headOfficeAddressLine2, string? headOfficeAddressTown, string? headOfficeAddressCounty, string? headOfficeAddressPostcode, string? headOfficeAddressCountry, string? headOfficeAddressTelephone, string? ofqualStatus, DateTime? ofqualRecognisedFrom, DateTime? ofqualRecognisedTo)
        {
            RecognitionNumber = recognitionNumber;
            Name = name;
            LegalName = legalName;
            Acronym = acronym;
            Email = email;
            Website = website;
            HeadOfficeAddressLine1 = headOfficeAddressLine1;
            HeadOfficeAddressLine2 = headOfficeAddressLine2;
            HeadOfficeAddressTown = headOfficeAddressTown;
            HeadOfficeAddressCounty = headOfficeAddressCounty;
            HeadOfficeAddressPostcode = headOfficeAddressPostcode;
            HeadOfficeAddressCountry = headOfficeAddressCountry;
            HeadOfficeAddressTelephone = headOfficeAddressTelephone;
            OfqualStatus = ofqualStatus;
            OfqualRecognisedFrom = ofqualRecognisedFrom;
            OfqualRecognisedTo = ofqualRecognisedTo;
        }
    }
}
