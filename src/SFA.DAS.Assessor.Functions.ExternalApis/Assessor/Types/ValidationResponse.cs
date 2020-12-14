using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class ValidationResponse
    {
        public ValidationResponse()
        {
            if (Errors == null) { Errors = new List<ValidationErrorDetail>(); }
        }

        public ValidationResponse(ValidationErrorDetail validationErrorDetail)
            : this()
        {
            Errors.Add(validationErrorDetail);
        }

        public List<ValidationErrorDetail> Errors { get; set; }
        public bool IsValid => Errors.Count == 0;
    }
}