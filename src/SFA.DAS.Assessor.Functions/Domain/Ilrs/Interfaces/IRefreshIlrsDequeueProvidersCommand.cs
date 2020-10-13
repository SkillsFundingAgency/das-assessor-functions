using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsDequeueProvidersCommand
    {
        ICollector<string> StorageQueue { get; set; }

        Task Execute(string message);
    }
}
