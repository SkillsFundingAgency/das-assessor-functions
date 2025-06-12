using Microsoft.Azure.WebJobs;
using SFA.DAS.Assessor.Functions.Domain.Interfaces;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsEnqueueProvidersCommand : ICommand
    {
        ICollector<string> StorageQueue { get; set; }
    }
}
