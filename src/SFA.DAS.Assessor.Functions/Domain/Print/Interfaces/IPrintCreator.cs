using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintCreator
    {
        void Create(int batchNumber, IEnumerable<Certificate> certificates, string file);
    }
}
