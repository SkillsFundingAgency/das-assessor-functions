﻿using SFA.DAS.Assessor.Functions.Domain.Print.Types;
using System.Collections.Generic;

namespace SFA.DAS.Assessor.Functions.Domain.Print.Interfaces
{
    public interface IPrintCreator
    {
        PrintOutput Create(int batchNumber, IEnumerable<Certificate> certificates);
    }
}
