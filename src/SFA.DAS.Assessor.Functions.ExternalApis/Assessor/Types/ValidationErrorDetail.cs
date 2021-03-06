﻿using System.Runtime.Serialization;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class ValidationErrorDetail
    {
        public ValidationErrorDetail()
        {
        }

        public ValidationErrorDetail(string field, string errorMessage)
        {
            Field = field;
            ErrorMessage = errorMessage;
        }

        public ValidationErrorDetail(string field, string errorMessage, ValidationStatusCode statusCode)
        {
            Field = field;
            ErrorMessage = errorMessage;
            ValidationStatusCode = statusCode;
        }

        public ValidationErrorDetail(string errorMessage, ValidationStatusCode statusCode)
        {
            ErrorMessage = errorMessage;
            ValidationStatusCode = statusCode;
        }

        public string Field { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationStatusCode ValidationStatusCode { get; set; }


        public string StatusCode => ValidationStatusCode.ToString();
    }

    public enum ValidationStatusCode
    {
        [EnumMember(Value = "BadRequest")]
        BadRequest,
        [EnumMember(Value = "AlreadyExists")]
        AlreadyExists,
        [EnumMember(Value = "NotFound")]
        NotFound,
        [EnumMember(Value = "Warning")]
        Warning
    }
}