using SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types;
using System;

namespace SFA.DAS.Assessor.Functions.ApiClient.Types
{
    public class EMailTemplate : BaseEntity
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }
        public string Recipients { get; set; }
        public string RecipientTemplate { get; set; }
    }
}
