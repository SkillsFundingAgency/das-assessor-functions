using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.ExternalApis.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetToken();
        Task<string> RefreshToken();
    }
}
