using System;
using MediatR;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class UpdateBatchDataBatchLogRequest : IRequest<ValidationResponse>
    {
        public Guid Id { get; set; }
        public BatchData BatchData { get; set; }
    }
}
