using MediatR;
using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class SendEmailRequest : IRequest
    {
        public SendEmailRequest(string email, EmailTemplateSummary emailTemplateSummary, dynamic tokens)
        {
            EmailTemplateSummary = emailTemplateSummary;
            Email = email;
            Tokens = tokens;
        }

        public EmailTemplateSummary EmailTemplateSummary { get; }
        public string Email { get; set; }
        public dynamic Tokens { get; }
    }

    public class EmailTemplateSummary
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateId { get; set; }
        public string Recipients { get; set; }        
    }
}
