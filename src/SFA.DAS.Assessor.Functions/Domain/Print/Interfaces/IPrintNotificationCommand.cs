
using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintNotificationCommand : ICommand
    {
        ICollector<string> StorageQueue { get; set; }
    }
}
