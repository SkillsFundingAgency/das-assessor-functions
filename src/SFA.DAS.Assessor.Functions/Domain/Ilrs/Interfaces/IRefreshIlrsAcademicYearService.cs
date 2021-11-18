using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Assessor.Functions.Domain.Ilrs.Interfaces
{
    public interface IRefreshIlrsAcademicYearService
    {
        Task<List<string>> ValidateAllAcademicYears(DateTime lastRunDateTime, DateTime currentRunDateTime);
    }
}
