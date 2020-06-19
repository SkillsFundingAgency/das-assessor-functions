using SFA.DAS.Assessor.Functions.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsEnqueueProvidersCommand : ICommand
    {
        IStorageQueue StorageQueue { get; set; }
    }
}
