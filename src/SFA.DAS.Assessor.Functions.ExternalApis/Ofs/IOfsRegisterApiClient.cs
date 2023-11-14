using SFA.DAS.Assessor.Functions.ExternalApis.Ofs.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Ofs
{
    public interface IOfsRegisterApiClient : IApiClientBase
    {
        Task<List<OfsProvider>> GetProviders();
    }
}