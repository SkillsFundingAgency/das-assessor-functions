using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using SFA.DAS.Assessor.Functions.Domain.Print.Types;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IDeliveryNotificationCommand : IQueueCommand<CertificatePrintStatusUpdateMessage>
    {
    }
}
