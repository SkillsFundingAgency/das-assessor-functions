using System;
using MediatR;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Assessor.Types
{
    public class PrintedBatchLogRequest : IRequest<ValidationResponse>
    {
        public int BatchNumber { get; set; }
        public DateTime PrintedAt { get; set; }
    }
}
