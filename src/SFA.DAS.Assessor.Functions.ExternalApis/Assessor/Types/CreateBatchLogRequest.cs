using MediatR;
using System;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class CreateBatchLogRequest : IRequest<BatchLogResponse>
    {
        public DateTime ScheduledDate { get; set; }
    }
}
