using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Assessor.Functions.Infrastructure.Options.OfqualImport
{
    [ExcludeFromCodeCoverage]
    public class OfqualImportOptions
    {
        public string DownloadBlobContainer { get; set; }
        public string OrganisationsDataUrl { get; set; }
        public string QualificationsDataUrl { get; set; }
    }
}
